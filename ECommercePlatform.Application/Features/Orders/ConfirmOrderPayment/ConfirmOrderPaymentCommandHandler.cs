using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace ECommercePlatform.Application.Features.Orders.ConfirmOrderPayment;

/// <summary>
/// Marks a payment Success only after backend verification.
/// Never trusts a browser-supplied status. Idempotent when already Success.
/// </summary>
public sealed class ConfirmOrderPaymentCommandHandler(
    IOrderRepository orders,
    IAdminRepository<Notification> notifications,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IPaymentVerificationService paymentVerification,
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

        // Idempotent: already captured — return as-is.
        if (payment.Status == PaymentStatus.Success)
        {
            return Result.Success(OrderMappings.ToDetail(order));
        }

        if (payment.Status is PaymentStatus.Refunded or PaymentStatus.Failed)
        {
            return Result.Failure<OrderDetailResponse>(
                Error.Conflict("orders.payment_not_confirmable", "This payment can no longer be confirmed."));
        }

        // COD is collected on delivery — the client can never mark it Success.
        if (payment.PaymentMethod == PaymentMethod.COD)
        {
            return Result.Failure<OrderDetailResponse>(
                Error.Validation("orders.cod_not_online", "COD payments are confirmed on delivery, not online."));
        }

        var gateway = payment.PaymentGateway ?? string.Empty;
        var reference = request.TransactionReference?.Trim();

        // Format: "gatewayPaymentId:signature" for Razorpay-style verification,
        // or a bare gateway payment id when the webhook already verified.
        string? gatewayPaymentId = null;
        string? signature = null;
        if (!string.IsNullOrWhiteSpace(reference))
        {
            var parts = reference.Split(':', 2);
            gatewayPaymentId = parts[0];
            signature = parts.Length == 2 ? parts[1] : null;
        }

        var verified = await paymentVerification.VerifyGatewayConfirmationAsync(
            payment.PaymentId.ToString(),
            gatewayPaymentId,
            signature,
            cancellationToken);

        if (!verified && !paymentVerification.AllowManualClientConfirm)
        {
            logger.LogWarning(
                "Rejected unverified payment confirmation for order {OrderId}.",
                request.OrderId);
            return Result.Failure<OrderDetailResponse>(
                Error.Validation(
                    "orders.payment_verification_required",
                    "Payment could not be verified with the gateway."));
        }

        var now = timeProvider.GetUtcNow();

        payment.Status = PaymentStatus.Success;
        payment.PaidAt = now.UtcDateTime;
        payment.GatewayTransactionId = gatewayPaymentId ?? payment.GatewayTransactionId;
        payment.UpdatedAt = now;

        if (order.OrderStatus is OrderStatus.Pending or OrderStatus.Confirmed)
        {
            order.OrderStatus = OrderStatus.Confirmed;
            order.UpdatedAt = now;

            orders.AddStatusHistory(new OrderStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                OrderId = order.OrderId,
                Status = OrderStatus.Confirmed,
                ChangedByUserId = request.UserId,
                Remarks = "Payment confirmed by gateway verification.",
                ChangedAt = now.UtcDateTime
            });
        }

        NotificationEmitter.Emit(
            notifications,
            "payment",
            $"Payment confirmed for order {OrderMappings.OrderNoFor(order)}",
            $"₹{payment.Amount} received via {payment.PaymentMethod}.",
            "/admin/orders");

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
                gatewayPaymentId),
            cancellationToken);

        return Result.Success(OrderMappings.ToDetail(order));
    }
}
