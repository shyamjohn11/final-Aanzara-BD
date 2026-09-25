using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class DeliveryRuleRepository : IDeliveryRuleRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKey = "delivery-rule:primary";

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public DeliveryRuleRepository(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // There is exactly one configuration row by convention — nothing in the
    // schema enforces that yet, so take the first one rather than assume an id.
    public async Task<DeliveryRule?> GetAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out DeliveryRule? cached))
        {
            return cached;
        }

        var rule = await _db.DeliveryRules.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        // Cache null as a sentinel so a missing row is not re-queried every call.
        _cache.Set(CacheKey, rule, CacheDuration);
        return rule;
    }
}
