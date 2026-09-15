using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(Guid UserId, Guid AddressId, string PaymentMethod)
    : ICommand<Result<PlaceOrderResponse>>;
