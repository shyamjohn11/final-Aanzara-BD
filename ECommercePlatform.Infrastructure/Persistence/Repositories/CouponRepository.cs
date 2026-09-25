using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _db;

    public CouponRepository(ApplicationDbContext db) => _db = db;

    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => _db.Coupons.FirstOrDefaultAsync(
            c => c.CouponCode == code,
            cancellationToken);

    public void Add(Coupon coupon) => _db.Coupons.Add(coupon);
}
