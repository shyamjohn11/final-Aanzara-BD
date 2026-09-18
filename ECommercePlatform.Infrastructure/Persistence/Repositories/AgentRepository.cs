using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class AgentRepository : IAgentRepository
{
    private readonly ApplicationDbContext _db;

    public AgentRepository(ApplicationDbContext db) => _db = db;

    public Task<Agent?> GetByIdAsync(Guid agentId, CancellationToken cancellationToken)
        => _db.Agents
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AgentId == agentId, cancellationToken);

    public async Task<IReadOnlyList<Agent>> GetByIdsAsync(
        IReadOnlyCollection<Guid> agentIds, CancellationToken cancellationToken)
    {
        if (agentIds.Count == 0)
        {
            return Array.Empty<Agent>();
        }

        return await _db.Agents
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => agentIds.Contains(a.AgentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Agent> Agents, int TotalCount)> SearchAsync(
        string? search, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Agents
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            // An unrecognised filter matches nothing rather than being ignored.
            if (!Enum.TryParse<AgentStatus>(status.Trim(), ignoreCase: true, out var parsed))
            {
                return (Array.Empty<Agent>(), 0);
            }

            query = query.Where(a => a.Status == parsed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.User.Name.Contains(term) ||
                a.User.Email.Contains(term) ||
                (a.EmployeeCode != null && a.EmployeeCode.Contains(term)));
        }

        var ordered = query.OrderByDescending(a => a.CreatedAt);

        var total = await ordered.CountAsync(cancellationToken);

        var agents = await ordered
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (agents, total);
    }
}
