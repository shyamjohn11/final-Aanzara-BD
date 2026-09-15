using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IWishlistRepository
{
    Task<WishlistItem?> GetByUserAndProductAsync(
        Guid userId, Guid productId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WishlistItem>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken);

    void Add(WishlistItem wishlistItem);

    void Remove(WishlistItem wishlistItem);
}