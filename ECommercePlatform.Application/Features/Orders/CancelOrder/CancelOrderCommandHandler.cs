using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Enums;

using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace ECommercePlatform.Application.Features.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEmailService emailService,
    ILogger<CancelOrderCommandHandler> logger)
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
        var refunded = payment?.Status == PaymentStatus.Success;
        if (refunded)
        {
            payment!.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = now;
        }

        var orderNo = OrderMappings.OrderNoFor(order);
        NotificationEmitter.Emit(
            notifications,
            "order",
            $"Order {orderNo} cancelled",
            string.IsNullOrWhiteSpace(request.Reason) ? "Order cancelled by customer." : request.Reason.Trim(),
            "/admin/orders");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort user email; failure must never fail cancellation.
        await OrderEmailSender.TrySendAsync(
            emailService,
            logger,
            order.User?.Email,
            $"Order {orderNo} cancelled",
            OrderEmailSender.CancelHtml(
                order.User?.Name ?? "Customer",
                orderNo,
                request.Reason,
                refunded),
            cancellationToken);

        return Result.Success(OrderMappings.ToDetail(order));
    }
}
