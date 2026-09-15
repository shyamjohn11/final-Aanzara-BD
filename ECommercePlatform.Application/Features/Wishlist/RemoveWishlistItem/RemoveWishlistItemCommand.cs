using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.RemoveWishlistItem;

public sealed record RemoveWishlistItemCommand(Guid UserId, Guid ProductId) : ICommand<Result>;