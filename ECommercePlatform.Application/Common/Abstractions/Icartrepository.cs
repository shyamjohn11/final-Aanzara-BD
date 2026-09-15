using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Cart cart);

    /// <summary>Loads the caller's cart, creating an empty one if this is their first add.</summary>
    Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Scoped by userId so one caller can never touch another's cart item by guessing an id.</summary>
    Task<CartItem?> GetItemByIdAndUserIdAsync(Guid cartItemId, Guid userId, CancellationToken cancellationToken);

    void AddItem(CartItem item);

    /// <summary>Marks a cart item for deletion; persisted on the next SaveChanges.</summary>
    void RemoveItem(CartItem item);
}