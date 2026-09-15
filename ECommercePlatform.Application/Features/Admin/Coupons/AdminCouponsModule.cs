using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Coupons;

// IDs #85-89 — GET list / GET by id / POST / PUT / DELETE, Admin-role.

public sealed record CouponAdminResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string DiscountType { get; init; } = "Percent";
    public decimal DiscountValue { get; init; }
    public decimal MinOrderValue { get; init; }
    public int UsageLimit { get; init; } = 1;
    public int UsedCount { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetCouponsQuery : IQuery<Result<PagedResult<CouponAdminResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetCouponByIdQuery(Guid Id) : IQuery<Result<CouponAdminResponse>>;

public sealed record CreateCouponCommand : ICommand<Result<CouponAdminResponse>>
{
    [MaxLength(50)] public string? Code { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(20)] public string? DiscountType { get; init; }
    [Range(0, 1000000)] public decimal DiscountValue { get; init; }
    [Range(0, 1000000)] public decimal MinOrderValue { get; init; }
    [Range(1, 1000000)] public int UsageLimit { get; init; } = 1;
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateCouponCommand : ICommand<Result<CouponAdminResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(50)] public string? Code { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(20)] public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal? MinOrderValue { get; init; }
    public int? UsageLimit { get; init; }
    public int? UsedCount { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteCouponCommand(Guid Id) : ICommand<Result>;

internal static class CouponAdminSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<CouponAdminResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            return new List<CouponAdminResponse>
            {
                new() { Id = Guid.NewGuid(), Code = "SAVE10", Title = "Save 10%", Description = " Flat 10% off", DiscountType = "Percent", DiscountValue = 10, MinOrderValue = 499, UsageLimit = 1, UsedCount = 124, StartsAt = now.AddDays(-10), EndsAt = now.AddDays(20), Status = "Active", CreatedAt = now.AddDays(-10), UpdatedAt = now },
                new() { Id = Guid.NewGuid(), Code = "FLAT100", Title = "Flat Rs.100 off", Description = "Orders above Rs.999", DiscountType = "Flat", DiscountValue = 100, MinOrderValue = 999, UsageLimit = 2, UsedCount = 57, StartsAt = now.AddDays(-30), EndsAt = now.AddDays(30), Status = "Active", CreatedAt = now.AddDays(-30), UpdatedAt = now },
            };
        });
    }
}

public sealed class GetCouponsQueryHandler
    : IQueryHandler<GetCouponsQuery, Result<PagedResult<CouponAdminResponse>>>
{
    public Task<Result<PagedResult<CouponAdminResponse>>> Handle(
        GetCouponsQuery request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        var filtered = AdminCrudStore<CouponAdminResponse>.All()
            .Where(c => AdminPaging.MatchesStatus(request.Status, c.Status))
            .Where(c => AdminPaging.Matches(request.Search, c.Code, c.Title, c.Description)
                || AdminPaging.MatchesExtra(request.Search, c.Extra))
            .OrderByDescending(c => c.CreatedAt);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class GetCouponByIdQueryHandler
    : IQueryHandler<GetCouponByIdQuery, Result<CouponAdminResponse>>
{
    public Task<Result<CouponAdminResponse>> Handle(
        GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        return Task.FromResult(
            AdminCrudStore<CouponAdminResponse>.TryGet(request.Id, out var v) && v is not null
                ? Result.Success(v)
                : Result.Failure<CouponAdminResponse>(AdminErrors.NotFound("Coupon", request.Id)));
    }
}

public sealed class CreateCouponCommandHandler
    : IRequestHandler<CreateCouponCommand, Result<CouponAdminResponse>>
{
    public Task<Result<CouponAdminResponse>> Handle(
        CreateCouponCommand request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Task.FromResult(Result.Failure<CouponAdminResponse>(
                Error.Validation("admin.coupon_code_required", "Coupon code is required.")));
        }

        var now = DateTimeOffset.UtcNow;
        var code = request.Code!.Trim().ToUpperInvariant();
        var title = request.Title ?? request.Name ?? code;
        var response = new CouponAdminResponse
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = title,
            Description = request.Description,
            DiscountType = string.IsNullOrWhiteSpace(request.DiscountType) ? "Percent" : request.DiscountType!,
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            UsageLimit = request.UsageLimit <= 0 ? 1 : request.UsageLimit,
            UsedCount = 0,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!,
            CreatedAt = now,
            UpdatedAt = now,
            Extra = request.Extra,
        };
        AdminCrudStore<CouponAdminResponse>.Put(response);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class UpdateCouponCommandHandler
    : IRequestHandler<UpdateCouponCommand, Result<CouponAdminResponse>>
{
    public Task<Result<CouponAdminResponse>> Handle(
        UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        if (!AdminCrudStore<CouponAdminResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<CouponAdminResponse>(AdminErrors.NotFound("Coupon", request.Id)));
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
            Code = request.Code?.Trim().ToUpperInvariant() ?? existing.Code,
            Title = request.Title ?? request.Name ?? existing.Title,
            Description = request.Description ?? existing.Description,
            DiscountType = request.DiscountType ?? existing.DiscountType,
            DiscountValue = request.DiscountValue ?? existing.DiscountValue,
            MinOrderValue = request.MinOrderValue ?? existing.MinOrderValue,
            UsageLimit = request.UsageLimit ?? existing.UsageLimit,
            UsedCount = request.UsedCount ?? existing.UsedCount,
            StartsAt = request.StartsAt ?? existing.StartsAt,
            EndsAt = request.EndsAt ?? existing.EndsAt,
            Status = request.Status ?? existing.Status,
            UpdatedAt = DateTimeOffset.UtcNow,
            Extra = mergedExtra,
        };
        AdminCrudStore<CouponAdminResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}

public sealed class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, Result>
{
    public Task<Result> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        if (!AdminCrudStore<CouponAdminResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Coupon", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}
