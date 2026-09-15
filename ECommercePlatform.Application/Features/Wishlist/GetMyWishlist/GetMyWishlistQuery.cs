using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Wishlist.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.GetMyWishlist;

public sealed record GetMyWishlistQuery(Guid UserId) : IQuery<Result<IReadOnlyList<WishlistItemResponse>>>;