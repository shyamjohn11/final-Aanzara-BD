using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Agents;

// Real Agent rows (Agents table) with their owning user and dealer count,
// backing Admin → Agents → Dealers navigation. Agent onboarding applications
// stay on the separate agent-onboarding flow.

// IDs #156-157 — GET list / GET by id, Admin-role.

public sealed record AgentResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? EmployeeCode { get; init; }
    public string Status { get; init; } = string.Empty;
    public int DealerCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetAgentsQuery : IQuery<Result<PagedResult<AgentResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetAgentByIdQuery(Guid Id) : IQuery<Result<AgentResponse>>;

internal static class AgentMappings
{
    internal static AgentResponse ToDto(Agent agent, int dealerCount) => new()
    {
        Id = agent.AgentId,
        AgentId = agent.AgentId,
        UserId = agent.UserId,
        Name = agent.User?.Name ?? "Unknown",
        Email = agent.User?.Email,
        Phone = agent.User?.Phone,
        EmployeeCode = agent.EmployeeCode,
        Status = agent.Status.ToString(),
        DealerCount = dealerCount,
        CreatedAt = agent.CreatedAt,
        UpdatedAt = agent.UpdatedAt
    };
}

public sealed class GetAgentsQueryHandler(
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers)
    : IQueryHandler<GetAgentsQuery, Result<PagedResult<AgentResponse>>>
{
    public async Task<Result<PagedResult<AgentResponse>>> Handle(
        GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        var (rows, total) = await agents.SearchAsync(
            request.Search, request.Status, page, pageSize, cancellationToken);

        // One batch query for every dealer on this page, grouped client-side.
        var agentIds = rows.Select(a => a.AgentId).ToArray();
        var counts = new Dictionary<Guid, int>();
        if (agentIds.Length > 0)
        {
            var pageDealers = await dealers.ListAsync(
                d => agentIds.Contains(d.AgentId),
                q => q.OrderBy(d => d.CreatedAt),
                cancellationToken);

            foreach (var group in pageDealers.GroupBy(d => d.AgentId))
            {
                counts[group.Key] = group.Count();
            }
        }

        var items = rows.Select(a => AgentMappings.ToDto(
            a, counts.TryGetValue(a.AgentId, out var count) ? count : 0)).ToList();

        return Result.Success(new PagedResult<AgentResponse>(items, page, pageSize, total));
    }
}

public sealed class GetAgentByIdQueryHandler(
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers)
    : IQueryHandler<GetAgentByIdQuery, Result<AgentResponse>>
{
    public async Task<Result<AgentResponse>> Handle(
        GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await agents.GetByIdAsync(request.Id, cancellationToken);
        if (agent is null)
        {
            return Result.Failure<AgentResponse>(AdminErrors.NotFound("Agent", request.Id));
        }

        var dealerCount = await dealers.CountAsync(
            d => d.AgentId == agent.AgentId, cancellationToken);

        return Result.Success(AgentMappings.ToDto(agent, dealerCount));
    }
}
