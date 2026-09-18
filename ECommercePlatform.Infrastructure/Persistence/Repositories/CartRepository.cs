using System.Data;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;

    public CartRepository(ApplicationDbContext db) => _db = db;

    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => _db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
            .Include(c => c.AppliedCoupon)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public void Add(Cart cart) => _db.Carts.Add(cart);

    // The unique index IX_Carts_userId is the arbiter of a race: two concurrent
    // add-to-cart calls must not both insert a cart for the same user and die
    // with a duplicate-key exception. The INSERT...WHERE NOT EXISTS with
    // UPDLOCK/HOLDLOCK serializes cart creation for one user at the database,
    // so every concurrent caller ends up sharing the same existing row.
    public async Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var userIdValue = userId.ToString();

        await _db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO Carts (cartId, userId, appliedCouponId, createdAt, updatedAt)
            SELECT @cartId, @userId, NULL, @createdAt, @updatedAt
            WHERE NOT EXISTS (
                SELECT 1 FROM Carts WITH (UPDLOCK, HOLDLOCK) WHERE userId = @userId
            )
            """,
            new object[]
            {
                new SqlParameter("@cartId", SqlDbType.Char, 36) { Value = userIdValue },
                new SqlParameter("@userId", SqlDbType.Char, 36) { Value = userIdValue },
                new SqlParameter("@createdAt", SqlDbType.DateTimeOffset) { Value = now },
                new SqlParameter("@updatedAt", SqlDbType.DateTimeOffset) { Value = now },
            },
            cancellationToken);

        return await _db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
            .Include(c => c.AppliedCoupon)
            .FirstAsync(c => c.UserId == userId, cancellationToken);
    }

public Task<CartItem?> GetItemByIdAndUserIdAsync(Guid cartItemId, Guid userId, CancellationToken cancellationToken)
    => _db.CartItems
        .Include(i => i.Product)
        .Include(i => i.Cart)
        .FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.Cart.UserId == userId, cancellationToken);

    public void AddItem(CartItem item) => _db.CartItems.Add(item);

    public void RemoveItem(CartItem item) => _db.CartItems.Remove(item);
}