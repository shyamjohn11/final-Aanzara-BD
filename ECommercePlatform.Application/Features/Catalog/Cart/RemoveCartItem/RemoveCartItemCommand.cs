using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid UserId, Guid CartItemId)
    : ICommand<Result>;
