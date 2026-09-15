using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.UpdateCartQuantity;

public sealed record UpdateCartQuantityCommand(Guid UserId, Guid CartItemId, int Quantity)
    : ICommand<Result<CartItemResponse>>;