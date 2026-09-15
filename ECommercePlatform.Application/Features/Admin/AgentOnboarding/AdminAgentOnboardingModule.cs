using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.AgentOnboarding;

// IDs #116-120 + #121 PATCH /{id}/status (approve/reject), Admin-role.

public sealed record AgentOnboardingResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string ShopName { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetAgentOnboardingQuery : IQuery<Result<PagedResult<AgentOnboardingResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetAgentOnboardingByIdQuery(Guid Id) : IQuery<Result<AgentOnboardingResponse>>;

public sealed record CreateAgentOnboardingCommand : ICommand<Result<AgentOnboardingResponse>>
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(200)] public string? ShopName { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateAgentOnboardingCommand : ICommand<Result<AgentOnboardingResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(200)] public string? ShopName { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteAgentOnboardingCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateAgentOnboardingStatusCommand(Guid Id, string Status)
    : ICommand<Result<AgentOnboardingResponse>>;

internal static class AgentOnboardingSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<AgentOnboardingResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            return new List<AgentOnboardingResponse>
            {
                new() { Id = Guid.NewGuid(), Name = "Amit Verma", Email = "amit@agent.in", Phone = "9810000001", ShopName = "Verma Store", Address = "MG Road, Bengaluru", Status = "Pending", AppliedAt = now.AddDays(-4), CreatedAt = now.AddDays(-4), UpdatedAt = now },
                new() { Id = Guid.NewGuid(), Name = "Priya Nair", Email = "priya@agent.in", Phone = "9810000002", ShopName = "Nair Mart", Address = "Kochi, Kerala", Status = "Approved", AppliedAt = now.AddDays(-14), CreatedAt = now.AddDays(-14), UpdatedAt = now },
            };
        });
    }
}

public sealed class GetAgentOnboardingQueryHandler
    : IQueryHandler<GetAgentOnboardingQuery, Result<PagedResult<AgentOnboardingResponse>>>
{
    public Task<Result<PagedResult<AgentOnboardingResponse>>> Handle(
        GetAgentOnboardingQuery request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        var filtered = AdminCrudStore<AgentOnboardingResponse>.All()
            .Where(a => AdminPaging.MatchesStatus(request.Status, a.Status))
            .Where(a => AdminPaging.Matches(request.Search, a.Name, a.Email, a.Phone, a.ShopName, a.Address)
                || AdminPaging.MatchesExtra(request.Search, a.Extra))
            .OrderByDescending(a => a.AppliedAt);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class GetAgentOnboardingByIdQueryHandler
    : IQueryHandler<GetAgentOnboardingByIdQuery, Result<AgentOnboardingResponse>>
{
    public Task<Result<AgentOnboardingResponse>> Handle(
        GetAgentOnboardingByIdQuery request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        return Task.FromResult(
            AdminCrudStore<AgentOnboardingResponse>.TryGet(request.Id, out var v) && v is not null
                ? Result.Success(v)
                : Result.Failure<AgentOnboardingResponse>(AdminErrors.NotFound("Agent onboarding", request.Id)));
    }
}

public sealed class CreateAgentOnboardingCommandHandler
    : IRequestHandler<CreateAgentOnboardingCommand, Result<AgentOnboardingResponse>>
{
    public Task<Result<AgentOnboardingResponse>> Handle(
        CreateAgentOnboardingCommand request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        var now = DateTimeOffset.UtcNow;
        var response = new AgentOnboardingResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name ?? "Unnamed agent",
            Email = request.Email,
            Phone = request.Phone,
            ShopName = request.ShopName ?? "Shop",
            Address = request.Address,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status!,
            AppliedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            Extra = request.Extra,
        };
        AdminCrudStore<AgentOnboardingResponse>.Put(response);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class UpdateAgentOnboardingCommandHandler
    : IRequestHandler<UpdateAgentOnboardingCommand, Result<AgentOnboardingResponse>>
{
    public Task<Result<AgentOnboardingResponse>> Handle(
        UpdateAgentOnboardingCommand request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        if (!AdminCrudStore<AgentOnboardingResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<AgentOnboardingResponse>(AdminErrors.NotFound("Agent onboarding", request.Id)));
        }

        var mergedExtra = existing.Extra;
        if (request.Extra is not null)
        {
            mergedExtra = new Dictionary<string, JsonElement>(mergedExtra ?? new());
            foreach (var kv in request.Extra)
            {
                mergedExtra[kv.Key] = kv.Value;
            }
        }

        var updated = existing with
        {
            Name = request.Name ?? existing.Name,
            Email = request.Email ?? existing.Email,
            Phone = request.Phone ?? existing.Phone,
            ShopName = request.ShopName ?? existing.ShopName,
            Address = request.Address ?? existing.Address,
            Status = request.Status ?? existing.Status,
            UpdatedAt = DateTimeOffset.UtcNow,
            Extra = mergedExtra,
        };
        AdminCrudStore<AgentOnboardingResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}

public sealed class DeleteAgentOnboardingCommandHandler
    : IRequestHandler<DeleteAgentOnboardingCommand, Result>
{
    public Task<Result> Handle(DeleteAgentOnboardingCommand request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        if (!AdminCrudStore<AgentOnboardingResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Agent onboarding", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}

public sealed class UpdateAgentOnboardingStatusCommandHandler
    : IRequestHandler<UpdateAgentOnboardingStatusCommand, Result<AgentOnboardingResponse>>
{
    public Task<Result<AgentOnboardingResponse>> Handle(
        UpdateAgentOnboardingStatusCommand request, CancellationToken cancellationToken)
    {
        AgentOnboardingSeeds.Ensure();
        if (!AdminCrudStore<AgentOnboardingResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<AgentOnboardingResponse>(AdminErrors.NotFound("Agent onboarding", request.Id)));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Task.FromResult(Result.Failure<AgentOnboardingResponse>(
                Error.Validation("admin.status_required", "Status is required.")));
        }

        var updated = existing with { Status = request.Status.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        AdminCrudStore<AgentOnboardingResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}
