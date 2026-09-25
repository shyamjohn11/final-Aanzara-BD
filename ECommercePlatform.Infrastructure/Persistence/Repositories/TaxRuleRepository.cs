using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class TaxRuleRepository : ITaxRuleRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKey = "tax-rules:all";

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public TaxRuleRepository(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<TaxRule>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyCollection<TaxRule>? cached) && cached is not null)
        {
            return cached;
        }

        var rules = await _db.TaxRules.AsNoTracking().ToListAsync(cancellationToken);
        IReadOnlyCollection<TaxRule> result = rules;
        _cache.Set(CacheKey, result, CacheDuration);
        return result;
    }
}
