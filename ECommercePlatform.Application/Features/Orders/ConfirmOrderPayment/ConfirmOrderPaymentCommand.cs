using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.ConfirmOrderPayment;

public sealed record ConfirmOrderPaymentCommand(Guid UserId, Guid OrderId, string? TransactionReference)
    : ICommand<Result<OrderDetailResponse>>;
