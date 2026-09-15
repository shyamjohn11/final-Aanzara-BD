using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.GetCart;

/// <summary>The caller's own cart. The id comes from the token, never the request.</summary>
public sealed record GetCartQuery(Guid UserId) : IQuery<Result<CartResponse>>;