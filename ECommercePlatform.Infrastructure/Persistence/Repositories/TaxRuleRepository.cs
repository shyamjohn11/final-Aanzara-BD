using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class TaxRuleRepository : ITaxRuleRepository
{
    private readonly ApplicationDbContext _db;

    public TaxRuleRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<TaxRule>> GetAllAsync(CancellationToken cancellationToken)
        => await _db.TaxRules.AsNoTracking().ToListAsync(cancellationToken);
}