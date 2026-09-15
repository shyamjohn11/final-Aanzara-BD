using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Orders;
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

    public GetMyNotificationsQueryHandler(IOrderRepository orders) => _orders = orders;

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

        return Result.Success<IReadOnlyList<CustomerNotificationResponse>>(feed);
    }
}
