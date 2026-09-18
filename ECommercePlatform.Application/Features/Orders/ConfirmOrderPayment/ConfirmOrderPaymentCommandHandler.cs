using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Enums;

using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace ECommercePlatform.Application.Features.Orders.ConfirmOrderPayment;

/// <summary>
/// Marks the order's payment as captured. The "Manual" gateway records no real
/// charge, so confirmation is what moves a paid order out of Pending.
/// </summary>
public sealed class ConfirmOrderPaymentCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEmailService emailService,
    ILogger<ConfirmOrderPaymentCommandHandler> logger)
    : ICommandHandler<ConfirmOrderPaymentCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        ConfirmOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.UserId != request.UserId)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound);
        }

        var payment = order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        if (payment is null)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.PaymentNotFound);
        }

        // Confirming twice is a no-op, not an error — retries must be safe.
        if (payment.Status != PaymentStatus.Success)
        {
            var now = timeProvider.GetUtcNow();

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = now.UtcDateTime;
            payment.GatewayTransactionId = request.TransactionReference;
            payment.UpdatedAt = now;

            order.OrderStatus = OrderStatus.Confirmed;
            order.UpdatedAt = now;

            orders.AddStatusHistory(new OrderStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                OrderId = order.OrderId,
                Status = OrderStatus.Confirmed,
                ChangedByUserId = request.UserId,
                Remarks = "Payment confirmed.",
                ChangedAt = now.UtcDateTime
            });

            NotificationEmitter.Emit(
                notifications,
                "payment",
                $"Payment confirmed for order {OrderMappings.OrderNoFor(order)}",
                $"₹{payment.Amount} received via {payment.PaymentMethod}.",
                "/admin/orders");

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Best-effort user email; failure must never fail confirmation.
            await OrderEmailSender.TrySendAsync(
                emailService,
                logger,
                order.User?.Email,
                $"Payment confirmed for order {OrderMappings.OrderNoFor(order)}",
                OrderEmailSender.PaymentConfirmedHtml(
                    order.User?.Name ?? "Customer",
                    OrderMappings.OrderNoFor(order),
                    payment.Amount,
                    payment.PaymentMethod.ToString(),
                    request.TransactionReference),
                cancellationToken);
        }

        return Result.Success(OrderMappings.ToDetail(order));
    }
}
