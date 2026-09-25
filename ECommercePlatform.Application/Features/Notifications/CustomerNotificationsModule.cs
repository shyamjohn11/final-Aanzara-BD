using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Orders;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Notifications;

// Customer feed derived from the caller's own order history — no new tables.
// IDs are deterministic (order + status + timestamp) so clients can persist
// read state locally.

public sealed record CustomerNotificationResponse
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = "order";
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsRead { get; init; }
    public string? Link { get; init; }
}

public sealed record GetMyNotificationsQuery(Guid UserId, int Count = 20)
    : IQuery<Result<IReadOnlyList<CustomerNotificationResponse>>>;

public sealed class GetMyNotificationsQueryHandler
    : IQueryHandler<GetMyNotificationsQuery, Result<IReadOnlyList<CustomerNotificationResponse>>>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly IAdminRepository<Enquiry> _enquiries;
    private readonly IAdminRepository<PricingRequest> _pricingRequests;
    private readonly IProductRepository _products;
    private readonly TimeProvider _timeProvider;

    /// <summary>Only products created inside this window count as arrivals.</summary>
    private const int NewArrivalDays = 30;

    /// <summary>Cap per feed so a bulk import cannot flood the bell.</summary>
    private const int MaxNewArrivals = 5;

    public GetMyNotificationsQueryHandler(
        IOrderRepository orders,
        IUserRepository users,
        IAdminRepository<Enquiry> enquiries,
        IAdminRepository<PricingRequest> pricingRequests,
        IProductRepository products,
        TimeProvider timeProvider)
    {
        _orders = orders;
        _users = users;
        _enquiries = enquiries;
        _pricingRequests = pricingRequests;
        _products = products;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyList<CustomerNotificationResponse>>> Handle(
        GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 20 : request.Count, 1, 50);
        var (orders, _) = await _orders.GetByUserIdAsync(
            request.UserId, 1, 50, cancellationToken);

        var feed = orders
            .SelectMany(o => o.StatusHistory
                .Select(h => (Order: o, Event: h)))
            .OrderByDescending(x => x.Event.ChangedAt)
            .Take(count)
            .Select(x =>
            {
                var status = x.Event.Status.ToString();
                var orderNo = OrderMappings.OrderNoFor(x.Order);
                var changedAt = x.Event.ChangedAt;
                var type = status switch
                {
                    "Shipped" or "OutForDelivery" or "Delivered" => "delivery",
                    _ => "order",
                };
                return new CustomerNotificationResponse
                {
                    Id = $"order-{x.Order.OrderId:N}-{status}-{changedAt:yyyyMMddHHmmss}",
                    Type = type,
                    Title = $"Order {orderNo} — {status}",
                    Message = x.Event.Remarks,
                    CreatedAt = changedAt,
                    IsRead = false,
                    Link = $"/orders?order={x.Order.OrderId}",
                };
            })
            .ToList();

        // Enquiry + bulk-quote status updates, matched to the account by
        // email (requests can also come from guests, who simply have no
        // account feed). IDs stay deterministic so clients can persist
        // read state locally, same as order rows.
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        var email = user?.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var lowered = email.ToLowerInvariant();

            var enquiries = await _enquiries.PageAsync(
                e => e.Email != null && e.Email.ToLower() == lowered,
                q => q.OrderByDescending(e => e.UpdatedAt),
                0, 50, cancellationToken);

            feed.AddRange(enquiries.Select(e => new CustomerNotificationResponse
            {
                Id = $"enquiry-{e.Id:N}-{e.Status}-{e.UpdatedAt:yyyyMMddHHmmss}",
                Type = "enquiry",
                Title = $"Enquiry update — {e.Status}",
                Message = $"Your enquiry \"{e.Subject}\" is now {e.Status}.",
                CreatedAt = e.UpdatedAt,
                IsRead = false,
                Link = null,
            }));

            var quotes = await _pricingRequests.PageAsync(
                p => p.Email != null && p.Email.ToLower() == lowered,
                q => q.OrderByDescending(p => p.UpdatedAt),
                0, 50, cancellationToken);

            feed.AddRange(quotes.Select(p => new CustomerNotificationResponse
            {
                Id = $"pricing-{p.Id:N}-{p.Status}-{p.UpdatedAt:yyyyMMddHHmmss}",
                Type = "pricing",
                Title = $"Bulk quote update — {p.Status}",
                Message = $"Your quote request for {p.Product} × {p.Quantity} is now {p.Status}.",
                CreatedAt = p.UpdatedAt,
                IsRead = false,
                Link = null,
            }));
        }

        // New arrivals for every user: latest Active products from the last
        // 30 days, capped so a bulk import cannot flood the bell. Typed as
        // "offer" so it renders on every surface (bell, /alerts, account
        // alerts) and honors the offers notification preference.
        var cutoff = _timeProvider.GetUtcNow().AddDays(-NewArrivalDays);
        var arrivals = await _products.SearchAsync(
            new ProductFilter(Status: ProductStatus.Active, SortBy: "created", SortDescending: true),
            1, 25, cancellationToken);

        feed.AddRange(arrivals.Items
            .Where(p => p.CreatedAt >= cutoff)
            .Take(MaxNewArrivals)
            .Select(p => new CustomerNotificationResponse
            {
                Id = $"newarrival-{p.ProductId:N}-{p.CreatedAt:yyyyMMddHHmmss}",
                Type = "offer",
                Title = $"New arrival: {p.ProductName}",
                Message = $"Just landed at ₹{p.Price:0.00} — tap to view.",
                CreatedAt = p.CreatedAt,
                IsRead = false,
                Link = $"/product/{p.ProductId}",
            }));

        var merged = feed
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToList();

        return Result.Success<IReadOnlyList<CustomerNotificationResponse>>(merged);
    }
}
