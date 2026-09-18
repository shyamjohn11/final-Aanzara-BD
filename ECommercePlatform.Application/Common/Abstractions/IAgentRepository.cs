using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IAgentRepository
{
    Task<Agent?> GetByIdAsync(Guid agentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Agent>> GetByIdsAsync(
        IReadOnlyCollection<Guid> agentIds, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Agent> Agents, int TotalCount)> SearchAsync(
        string? search, string? status, int page, int pageSize, CancellationToken cancellationToken);
}
