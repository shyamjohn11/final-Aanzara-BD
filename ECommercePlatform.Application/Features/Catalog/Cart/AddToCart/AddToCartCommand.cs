using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.AddToCart;

public sealed record AddToCartCommand(Guid UserId, Guid ProductId, int Quantity)
    : ICommand<Result<CartItemResponse>>;