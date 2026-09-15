using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Enums;

using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Domain.Entities;
namespace ECommercePlatform.Application.Features.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CancelOrderCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.UserId != request.UserId)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound);
        }

        // Once the warehouse has picked or shipped, cancellation is an admin
        // return flow, not a customer click.
        if (order.OrderStatus is not (OrderStatus.Pending or OrderStatus.Confirmed))
        {
            return Result.Failure<OrderDetailResponse>(
                OrderErrors.NotCancellable(order.OrderStatus.ToString()));
        }

        var now = timeProvider.GetUtcNow();

        order.OrderStatus = OrderStatus.Cancelled;
        order.UpdatedAt = now;

        orders.AddStatusHistory(new OrderStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Status = OrderStatus.Cancelled,
            ChangedByUserId = request.UserId,
            Remarks = string.IsNullOrWhiteSpace(request.Reason) ? "Order cancelled." : request.Reason.Trim(),
            ChangedAt = now.UtcDateTime
        });

        // Money already captured must come back when the order dies.
        var payment = order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        if (payment?.Status == PaymentStatus.Success)
        {
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = now;
        }

        NotificationEmitter.Emit(
            notifications,
            "order",
            $"Order {OrderMappings.OrderNoFor(order)} cancelled",
            string.IsNullOrWhiteSpace(request.Reason) ? "Order cancelled by customer." : request.Reason.Trim(),
            "/admin/orders");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(OrderMappings.ToDetail(order));
    }
}
