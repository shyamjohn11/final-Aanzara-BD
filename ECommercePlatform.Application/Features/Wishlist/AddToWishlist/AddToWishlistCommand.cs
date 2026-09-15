using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.AddToWishlist;

public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : ICommand<Result>;