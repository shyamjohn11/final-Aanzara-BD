using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IAgentRepository
{
    Task<Agent?> GetByIdAsync(Guid agentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Agent>> GetByIdsAsync(
        IReadOnlyCollection<Guid> agentIds, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Agent> Agents, int TotalCount)> SearchAsync(
        string? search, string? status, int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> EmployeeCodeExistsAsync(
        string employeeCode, Guid? excludingAgentId, CancellationToken cancellationToken);

    Task AddAsync(Agent agent, CancellationToken cancellationToken);

    Task RemoveAsync(Agent agent, CancellationToken cancellationToken);
}
