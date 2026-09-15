using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Orders;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Orders;

// Real order data for the admin console: search + status filter + paging,
// recent orders, status transitions with history, and delete. Reads go through
// IOrderRepository; writes save through IUnitOfWork like every other module.

public sealed record OrderLineDto
{
    public string Product { get; init; } = string.Empty;
    public int Qty { get; init; } = 1;
    public decimal Price { get; init; }
}

public sealed record OrderAdminResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string Customer { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public List<OrderLineDto> Items { get; init; } = new();
    public decimal Amount { get; init; }
    public string Payment { get; init; } = "COD";
    public string Status { get; init; } = "Pending";
    public DateTimeOffset Date { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetAdminOrdersQuery : IQuery<Result<PagedResult<OrderAdminResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetAdminOrderByIdQuery(Guid OrderId) : IQuery<Result<OrderAdminResponse>>;

public sealed record GetRecentAdminOrdersQuery(int Count = 5) : IQuery<Result<IReadOnlyList<OrderAdminResponse>>>;

public sealed record UpdateAdminOrderStatusCommand(Guid OrderId, string Status)
    : ICommand<Result<OrderAdminResponse>>;

public sealed record DeleteAdminOrderCommand(Guid OrderId) : ICommand<Result>;

internal static class OrderAdminMappings
{
    // Matches the customer-facing pipeline; "Processing" was a seed-only state.
    internal static readonly string[] AllowedStatuses =
        ["Pending", "Confirmed", "Shipped", "Delivered", "Cancelled", "Returned"];

    internal static OrderAdminResponse ToAdmin(Order order)
    {
        var payment = order.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        return new OrderAdminResponse
        {
            Id = order.OrderId,
            OrderId = order.OrderId,
            OrderNo = OrderMappings.OrderNoFor(order),
            Customer = order.User?.Name ?? "Unknown",
            CustomerEmail = order.User?.Email,
            Items = order.OrderItems
                .Select(i => new OrderLineDto
                {
                    Product = i.Product?.ProductName ?? "Item",
                    Qty = i.Quantity,
                    Price = i.UnitPrice,
                })
                .ToList(),
            Amount = order.GrandTotal,
            Payment = payment is null ? "COD" : payment.PaymentMethod.ToString(),
            Status = order.OrderStatus.ToString(),
            Date = order.CreatedAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
        };
    }
}

public sealed class GetAdminOrdersQueryHandler(
    IOrderRepository orders)
    : IQueryHandler<GetAdminOrdersQuery, Result<PagedResult<OrderAdminResponse>>>
{
    public async Task<Result<PagedResult<OrderAdminResponse>>> Handle(
        GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<OrderStatus>(request.Status.Trim(), ignoreCase: true, out var parsed))
            {
                // An unrecognised filter matches nothing rather than being ignored.
                return Result.Success(new PagedResult<OrderAdminResponse>([], request.Page, request.PageSize, 0));
            }

            status = parsed;
        }

        var (found, total) = await orders.SearchAsync(request.Search, status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(new PagedResult<OrderAdminResponse>(
            found.Select(OrderAdminMappings.ToAdmin).ToList(), request.Page, request.PageSize, total));
    }
}

public sealed class GetAdminOrderByIdQueryHandler(
    IOrderRepository orders)
    : IQueryHandler<GetAdminOrderByIdQuery, Result<OrderAdminResponse>>
{
    public async Task<Result<OrderAdminResponse>> Handle(
        GetAdminOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);

        return order is null
            ? Result.Failure<OrderAdminResponse>(AdminErrors.NotFound("Order", request.OrderId))
            : Result.Success(OrderAdminMappings.ToAdmin(order));
    }
}

public sealed class GetRecentAdminOrdersQueryHandler(
    IOrderRepository orders)
    : IQueryHandler<GetRecentAdminOrdersQuery, Result<IReadOnlyList<OrderAdminResponse>>>
{
    public async Task<Result<IReadOnlyList<OrderAdminResponse>>> Handle(
        GetRecentAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 5 : request.Count, 1, 50);
        var recent = await orders.GetRecentAsync(count, cancellationToken);

        return Result.Success<IReadOnlyList<OrderAdminResponse>>(
            recent.Select(OrderAdminMappings.ToAdmin).ToList());
    }
}

public sealed class UpdateAdminOrderStatusCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateAdminOrderStatusCommand, Result<OrderAdminResponse>>
{
    public async Task<Result<OrderAdminResponse>> Handle(
        UpdateAdminOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<OrderAdminResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        var status = request.Status.Trim();
        var match = OrderAdminMappings.AllowedStatuses
            .FirstOrDefault(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return Result.Failure<OrderAdminResponse>(AdminErrors.InvalidStatus(status));
        }

        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<OrderAdminResponse>(AdminErrors.NotFound("Order", request.OrderId));
        }

        var now = timeProvider.GetUtcNow();
        order.OrderStatus = Enum.Parse<OrderStatus>(match);
        orders.AddStatusHistory(new OrderStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Status = order.OrderStatus,
            ChangedByUserId = currentUser.UserId,
            Remarks = $"Status updated to {match} by admin.",
            ChangedAt = now.UtcDateTime,
        });

        NotificationEmitter.Emit(
            notifications,
            "order",
            $"Order {OrderMappings.OrderNoFor(order)} moved to {match}",
            $"Status updated to {match} by admin.",
            "/admin/orders");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(OrderAdminMappings.ToAdmin(order));
    }
}

public sealed class DeleteAdminOrderCommandHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminOrderCommand, Result>
{
    public async Task<Result> Handle(DeleteAdminOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(AdminErrors.NotFound("Order", request.OrderId));
        }

        // Children (items, payments, history, address snapshot) cascade.
        orders.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
