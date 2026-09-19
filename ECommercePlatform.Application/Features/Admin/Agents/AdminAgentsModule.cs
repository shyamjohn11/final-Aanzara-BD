using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
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

public sealed record CreateAgentCommand : ICommand<Result<AgentResponse>>
{
    [Required]
    [MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(30)]
    [Phone]
    public string? Phone { get; init; }

    [Required]
    [MinLength(12)]
    [MaxLength(256)]
    public string Passphrase { get; init; } = string.Empty;

    [MaxLength(50)]
    public string? EmployeeCode { get; init; }

    [MaxLength(20)]
    public string? Status { get; init; }
}

public sealed record UpdateAgentCommand : ICommand<Result<AgentResponse>>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    [MaxLength(150)]
    public string? Name { get; init; }

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; init; }

    [MaxLength(30)]
    [Phone]
    public string? Phone { get; init; }

    [MaxLength(50)]
    public string? EmployeeCode { get; init; }

    [MaxLength(20)]
    public string? Status { get; init; }
}

public sealed record DeleteAgentCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateAgentStatusCommand(Guid Id, string Status)
    : ICommand<Result<AgentResponse>>;

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

public sealed class CreateAgentCommandHandler(
    IUserRepository users,
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers,
    IRoleRepository roles,
    IUnitOfWork unitOfWork,
    IPassphraseHasher hasher,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<CreateAgentCommand, Result<AgentResponse>>
{
    public async Task<Result<AgentResponse>> Handle(
        CreateAgentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<AgentResponse>(
                Error.Validation("admin.agent_name_required", "Agent name is required."));
        }

        var email = request.Email.Trim();
        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<AgentResponse>(
                Error.Conflict("admin.email_taken", $"Email '{email}' is already registered."));
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeCode)
            && await agents.EmployeeCodeExistsAsync(
                request.EmployeeCode.Trim(), null, cancellationToken))
        {
            return Result.Failure<AgentResponse>(
                Error.Conflict("admin.employee_code_taken",
                    $"Employee code '{request.EmployeeCode.Trim()}' is already in use."));
        }

        if (!Enum.TryParse<AgentStatus>(
                request.Status?.Trim() ?? nameof(AgentStatus.Active),
                ignoreCase: true, out var status))
        {
            return Result.Failure<AgentResponse>(
                Error.Validation("admin.invalid_agent_status",
                    $"Status '{request.Status}' is not valid."));
        }

        var adminId = currentUser.UserId;
        if (adminId is null)
        {
            return Result.Failure<AgentResponse>(Error.Unauthorized(
                "admin.not_authenticated", "You must be signed in."));
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            HashedPassphrase = hasher.Hash(request.Passphrase),
            Status = UserStatus.Active,
            CreatedAt = timeProvider.GetUtcNow(),
            UpdatedAt = timeProvider.GetUtcNow()
        };

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign the Agent role so the new account can sign in as agent.
        var agentRole = await roles.GetByNameAsync("Agent", cancellationToken);
        if (agentRole is not null)
        {
            var alreadyHasRole = await roles.GetRolesForUserAsync(user.UserId, cancellationToken);
            if (!alreadyHasRole.Contains(agentRole.RoleName))
            {
                roles.AddAdminUserRole(user.UserId, agentRole.RoleId);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var agent = new Agent
        {
            AgentId = Guid.NewGuid(),
            UserId = user.UserId,
            CreatedByAdminId = adminId.Value,
            EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode)
                ? null
                : request.EmployeeCode.Trim(),
            Status = status,
            CreatedAt = timeProvider.GetUtcNow(),
            UpdatedAt = timeProvider.GetUtcNow(),
            User = user
        };

        // Use repository Add so the context tracks it correctly.
        await agents.AddAsync(agent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-load with includes for consistent DTO mapping.
        var created = await agents.GetByIdAsync(agent.AgentId, cancellationToken);
        return Result.Success(AgentMappings.ToDto(created ?? agent, 0));
    }
}

public sealed class UpdateAgentCommandHandler(
    IUserRepository users,
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAgentCommand, Result<AgentResponse>>
{
    public async Task<Result<AgentResponse>> Handle(
        UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await agents.GetByIdAsync(request.Id, cancellationToken);
        if (agent is null)
        {
            return Result.Failure<AgentResponse>(AdminErrors.NotFound("Agent", request.Id));
        }

        var user = agent.User ?? await users.GetByIdAsync(agent.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AgentResponse>(AdminErrors.NotFound("User", agent.UserId));
        }

        if (request.Email is not null)
        {
            var email = request.Email.Trim();
            if (!string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase)
                && await users.EmailExistsAsync(email, cancellationToken))
            {
                return Result.Failure<AgentResponse>(
                    Error.Conflict("admin.email_taken", $"Email '{email}' is already registered."));
            }

            user.Email = email;
        }

        if (request.Name is not null)
        {
            user.Name = request.Name.Trim();
        }

        if (request.Phone is not null)
        {
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        }

        if (request.EmployeeCode is not null)
        {
            var code = request.EmployeeCode.Trim();
            if (code.Length == 0)
            {
                agent.EmployeeCode = null;
            }
            else
            {
                if (await agents.EmployeeCodeExistsAsync(code, agent.AgentId, cancellationToken))
                {
                    return Result.Failure<AgentResponse>(
                        Error.Conflict("admin.employee_code_taken",
                            $"Employee code '{code}' is already in use."));
                }

                agent.EmployeeCode = code;
            }
        }

        if (request.Status is not null)
        {
            if (!Enum.TryParse<AgentStatus>(request.Status.Trim(), ignoreCase: true, out var status))
            {
                return Result.Failure<AgentResponse>(
                    Error.Validation("admin.invalid_agent_status",
                        $"Status '{request.Status}' is not valid."));
            }

            agent.Status = status;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dealerCount = await dealers.CountAsync(
            d => d.AgentId == agent.AgentId, cancellationToken);

        return Result.Success(AgentMappings.ToDto(agent, dealerCount));
    }
}

public sealed class DeleteAgentCommandHandler(
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAgentCommand, Result>
{
    public async Task<Result> Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await agents.GetByIdAsync(request.Id, cancellationToken);
        if (agent is null)
        {
            return Result.Failure(AdminErrors.NotFound("Agent", request.Id));
        }

        var dealerCount = await dealers.CountAsync(
            d => d.AgentId == agent.AgentId, cancellationToken);
        if (dealerCount > 0)
        {
            return Result.Failure(Error.Conflict("admin.agent_has_dealers",
                $"Agent has {dealerCount} dealer(s). Remove or reassign them first."));
        }

        await agents.RemoveAsync(agent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateAgentStatusCommandHandler(
    IAgentRepository agents,
    IAdminRepository<Dealer> dealers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAgentStatusCommand, Result<AgentResponse>>
{
    public async Task<Result<AgentResponse>> Handle(
        UpdateAgentStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<AgentResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        if (!Enum.TryParse<AgentStatus>(request.Status.Trim(), ignoreCase: true, out var status))
        {
            return Result.Failure<AgentResponse>(
                Error.Validation("admin.invalid_agent_status",
                    $"Status '{request.Status}' is not valid."));
        }

        var agent = await agents.GetByIdAsync(request.Id, cancellationToken);
        if (agent is null)
        {
            return Result.Failure<AgentResponse>(AdminErrors.NotFound("Agent", request.Id));
        }

        agent.Status = status;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dealerCount = await dealers.CountAsync(
            d => d.AgentId == agent.AgentId, cancellationToken);

        return Result.Success(AgentMappings.ToDto(agent, dealerCount));
    }
}
