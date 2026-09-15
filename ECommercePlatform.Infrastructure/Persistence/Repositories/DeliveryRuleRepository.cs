using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class DeliveryRuleRepository : IDeliveryRuleRepository
{
    private readonly ApplicationDbContext _db;

    public DeliveryRuleRepository(ApplicationDbContext db) => _db = db;

    // There is exactly one configuration row by convention — nothing in the
    // schema enforces that yet, so take the first one rather than assume an id.
    public Task<DeliveryRule?> GetAsync(CancellationToken cancellationToken)
        => _db.DeliveryRules.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
}