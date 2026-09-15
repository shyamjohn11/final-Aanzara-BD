using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class WishlistRepository : IWishlistRepository
{
    private readonly ApplicationDbContext _db;

    public WishlistRepository(ApplicationDbContext db) => _db = db;

    public Task<WishlistItem?> GetByUserAndProductAsync(
        Guid userId, Guid productId, CancellationToken cancellationToken)
        => _db.WishlistItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<WishlistItem>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken)
        => await _db.WishlistItems
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(WishlistItem wishlistItem) => _db.WishlistItems.Add(wishlistItem);

    public void Remove(WishlistItem wishlistItem) => _db.WishlistItems.Remove(wishlistItem);
}