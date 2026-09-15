using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ITaxRuleRepository
{
    /// <summary>
    /// Every configured rule. The table is small — a handful of category rates
    /// plus an optional category-less default — so callers load it whole and
    /// match per line item in memory rather than round-tripping per product.
    /// </summary>
    Task<IReadOnlyCollection<TaxRule>> GetAllAsync(CancellationToken cancellationToken);
}