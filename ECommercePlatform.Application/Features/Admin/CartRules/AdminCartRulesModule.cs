using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.CartRules;

// IDs #95-99 — GET list / GET by id / POST / PUT / DELETE, Admin-role.
// Persisted in SQL Server (CartRules table). Wire shape unchanged.

public sealed record CartRuleResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Condition { get; init; } = "None";
    public string Action { get; init; } = "None";
    public int Priority { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetCartRulesQuery : IQuery<Result<PagedResult<CartRuleResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetCartRuleByIdQuery(Guid Id) : IQuery<Result<CartRuleResponse>>;

public sealed record CreateCartRuleCommand : ICommand<Result<CartRuleResponse>>
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(2000)] public string? Condition { get; init; }
    [MaxLength(2000)] public string? Action { get; init; }
    [Range(0, int.MaxValue)] public int Priority { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateCartRuleCommand : ICommand<Result<CartRuleResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(2000)] public string? Condition { get; init; }
    [MaxLength(2000)] public string? Action { get; init; }
    public int? Priority { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteCartRuleCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateCartRuleStatusCommand(Guid Id, string Status) : ICommand<Result<CartRuleResponse>>;

internal static class CartRuleMappings
{
    internal static CartRuleResponse ToDto(this CartRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Title = rule.Title,
        Description = rule.Description,
        Condition = rule.Condition,
        Action = rule.Action,
        Priority = rule.Priority,
        StartsAt = rule.StartsAt,
        EndsAt = rule.EndsAt,
        Status = rule.Status,
        CreatedAt = rule.CreatedAt,
        UpdatedAt = rule.UpdatedAt
    };
}

public sealed class GetCartRulesQueryHandler
    : IQueryHandler<GetCartRulesQuery, Result<PagedResult<CartRuleResponse>>>
{
    private readonly IAdminRepository<CartRule> _rules;

    public GetCartRulesQueryHandler(IAdminRepository<CartRule> rules) => _rules = rules;

    public async Task<Result<PagedResult<CartRuleResponse>>> Handle(
        GetCartRulesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<CartRule, bool>> filter = AdminFilters.True<CartRule>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(r => r.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(r =>
                r.Name.Contains(s)
                || r.Title.Contains(s)
                || (r.Description != null && r.Description.Contains(s))
                || r.Condition.Contains(s)
                || r.Action.Contains(s));
        }

        var total = await _rules.CountAsync(filter, cancellationToken);
        var items = await _rules.PageAsync(
            filter,
            q => q.OrderBy(r => r.Priority).ThenByDescending(r => r.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<CartRuleResponse>(
            items.Select(r => r.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetCartRuleByIdQueryHandler
    : IQueryHandler<GetCartRuleByIdQuery, Result<CartRuleResponse>>
{
    private readonly IAdminRepository<CartRule> _rules;

    public GetCartRuleByIdQueryHandler(IAdminRepository<CartRule> rules) => _rules = rules;

    public async Task<Result<CartRuleResponse>> Handle(
        GetCartRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _rules.GetByIdAsync(request.Id, cancellationToken);

        return rule is null
            ? Result.Failure<CartRuleResponse>(AdminErrors.NotFound("Cart rule", request.Id))
            : Result.Success(rule.ToDto());
    }
}

public sealed class CreateCartRuleCommandHandler
    : IRequestHandler<CreateCartRuleCommand, Result<CartRuleResponse>>
{
    private readonly IAdminRepository<CartRule> _rules;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCartRuleCommandHandler(IAdminRepository<CartRule> rules, IUnitOfWork unitOfWork)
    {
        _rules = rules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartRuleResponse>> Handle(
        CreateCartRuleCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name ?? request.Title ?? "Untitled rule";
        var rule = new CartRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            Title = request.Title ?? name,
            Description = request.Description,
            Condition = string.IsNullOrWhiteSpace(request.Condition) ? "None" : request.Condition!,
            Action = string.IsNullOrWhiteSpace(request.Action) ? "None" : request.Action!,
            Priority = request.Priority,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!
        };

        _rules.Add(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}

public sealed class UpdateCartRuleCommandHandler
    : IRequestHandler<UpdateCartRuleCommand, Result<CartRuleResponse>>
{
    private readonly IAdminRepository<CartRule> _rules;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartRuleCommandHandler(IAdminRepository<CartRule> rules, IUnitOfWork unitOfWork)
    {
        _rules = rules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartRuleResponse>> Handle(
        UpdateCartRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _rules.GetByIdAsync(request.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<CartRuleResponse>(AdminErrors.NotFound("Cart rule", request.Id));
        }

        rule.Name = request.Name ?? request.Title ?? rule.Name;
        rule.Title = request.Title ?? request.Name ?? rule.Title;
        rule.Description = request.Description ?? rule.Description;
        rule.Condition = request.Condition ?? rule.Condition;
        rule.Action = request.Action ?? rule.Action;
        rule.Priority = request.Priority ?? rule.Priority;
        rule.StartsAt = request.StartsAt ?? rule.StartsAt;
        rule.EndsAt = request.EndsAt ?? rule.EndsAt;
        rule.Status = request.Status ?? rule.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}

public sealed class DeleteCartRuleCommandHandler : IRequestHandler<DeleteCartRuleCommand, Result>
{
    private readonly IAdminRepository<CartRule> _rules;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCartRuleCommandHandler(IAdminRepository<CartRule> rules, IUnitOfWork unitOfWork)
    {
        _rules = rules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCartRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _rules.GetByIdAsync(request.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure(AdminErrors.NotFound("Cart rule", request.Id));
        }

        _rules.Remove(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateCartRuleStatusCommandHandler
    : IRequestHandler<UpdateCartRuleStatusCommand, Result<CartRuleResponse>>
{
    private readonly IAdminRepository<CartRule> _rules;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartRuleStatusCommandHandler(IAdminRepository<CartRule> rules, IUnitOfWork unitOfWork)
    {
        _rules = rules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartRuleResponse>> Handle(
        UpdateCartRuleStatusCommand request, CancellationToken cancellationToken)
    {
        var rule = await _rules.GetByIdAsync(request.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<CartRuleResponse>(AdminErrors.NotFound("Cart rule", request.Id));
        }

        rule.Status = request.Status;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}
