using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.BusinessAccounts;

// IDs #110-114 + #115 PATCH /{id}/status (approve/block), Admin-role.

public sealed record BusinessAccountResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string ShopName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? GstNumber { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal Balance { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetBusinessAccountsQuery : IQuery<Result<PagedResult<BusinessAccountResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetBusinessAccountByIdQuery(Guid Id) : IQuery<Result<BusinessAccountResponse>>;

public sealed record CreateBusinessAccountCommand : ICommand<Result<BusinessAccountResponse>>
{
    [MaxLength(200)] public string? CompanyName { get; init; }
    [MaxLength(200)] public string? ShopName { get; init; }
    [MaxLength(200)] public string? OwnerName { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(20)] public string? GstNumber { get; init; }
    [Range(0, 100000000)] public decimal CreditLimit { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateBusinessAccountCommand : ICommand<Result<BusinessAccountResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? CompanyName { get; init; }
    [MaxLength(200)] public string? ShopName { get; init; }
    [MaxLength(200)] public string? OwnerName { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(20)] public string? GstNumber { get; init; }
    public decimal? CreditLimit { get; init; }
    public decimal? Balance { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteBusinessAccountCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateBusinessAccountStatusCommand(Guid Id, string Status)
    : ICommand<Result<BusinessAccountResponse>>;

internal static class BusinessAccountSeeds
{
    internal static readonly string[] AllowedStatuses =
        ["Pending", "Approved", "Blocked", "Active", "Inactive"];

    internal static void Ensure()
    {
        AdminCrudStore<BusinessAccountResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            return new List<BusinessAccountResponse>
            {
                new() { Id = Guid.NewGuid(), CompanyName = "Fresh Foods Pvt Ltd", ShopName = "Fresh Mart", OwnerName = "Ravi Kumar", Email = "ravi@freshmart.in", Phone = "9810012345", GstNumber = "29ABCDE1234F1Z5", CreditLimit = 100000, Balance = 25000, Status = "Pending", CreatedAt = now.AddDays(-20), UpdatedAt = now },
                new() { Id = Guid.NewGuid(), CompanyName = "Daily Needs Co", ShopName = "Daily Needs", OwnerName = "Sita Sharma", Email = "sita@dailyneeds.in", Phone = "9810012346", CreditLimit = 50000, Balance = 5000, Status = "Approved", CreatedAt = now.AddDays(-40), UpdatedAt = now },
            };
        });
    }
}

public sealed class GetBusinessAccountsQueryHandler
    : IQueryHandler<GetBusinessAccountsQuery, Result<PagedResult<BusinessAccountResponse>>>
{
    public Task<Result<PagedResult<BusinessAccountResponse>>> Handle(
        GetBusinessAccountsQuery request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        var filtered = AdminCrudStore<BusinessAccountResponse>.All()
            .Where(a => AdminPaging.MatchesStatus(request.Status, a.Status))
            .Where(a => AdminPaging.Matches(request.Search, a.CompanyName, a.ShopName, a.OwnerName, a.Email, a.Phone, a.GstNumber)
                || AdminPaging.MatchesExtra(request.Search, a.Extra))
            .OrderByDescending(a => a.CreatedAt);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class GetBusinessAccountByIdQueryHandler
    : IQueryHandler<GetBusinessAccountByIdQuery, Result<BusinessAccountResponse>>
{
    public Task<Result<BusinessAccountResponse>> Handle(
        GetBusinessAccountByIdQuery request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        return Task.FromResult(
            AdminCrudStore<BusinessAccountResponse>.TryGet(request.Id, out var v) && v is not null
                ? Result.Success(v)
                : Result.Failure<BusinessAccountResponse>(AdminErrors.NotFound("Business account", request.Id)));
    }
}

public sealed class CreateBusinessAccountCommandHandler
    : IRequestHandler<CreateBusinessAccountCommand, Result<BusinessAccountResponse>>
{
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBusinessAccountCommandHandler(
        IAdminRepository<Notification> notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BusinessAccountResponse>> Handle(
        CreateBusinessAccountCommand request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        var now = DateTimeOffset.UtcNow;
        var response = new BusinessAccountResponse
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName ?? "Untitled company",
            ShopName = request.ShopName ?? request.CompanyName ?? "Shop",
            OwnerName = request.OwnerName ?? "Owner",
            Email = request.Email,
            Phone = request.Phone,
            GstNumber = request.GstNumber,
            CreditLimit = request.CreditLimit,
            Balance = 0,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status!,
            CreatedAt = now,
            UpdatedAt = now,
            Extra = request.Extra,
        };
        AdminCrudStore<BusinessAccountResponse>.Put(response);
        NotificationEmitter.Emit(
            _notifications,
            "account",
            $"New business account: {response.CompanyName}",
            $"Status: {response.Status}.",
            "/admin/business-accounts");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(response);
    }
}

public sealed class UpdateBusinessAccountCommandHandler
    : IRequestHandler<UpdateBusinessAccountCommand, Result<BusinessAccountResponse>>
{
    public Task<Result<BusinessAccountResponse>> Handle(
        UpdateBusinessAccountCommand request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        if (!AdminCrudStore<BusinessAccountResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<BusinessAccountResponse>(AdminErrors.NotFound("Business account", request.Id)));
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
            CompanyName = request.CompanyName ?? existing.CompanyName,
            ShopName = request.ShopName ?? existing.ShopName,
            OwnerName = request.OwnerName ?? existing.OwnerName,
            Email = request.Email ?? existing.Email,
            Phone = request.Phone ?? existing.Phone,
            GstNumber = request.GstNumber ?? existing.GstNumber,
            CreditLimit = request.CreditLimit ?? existing.CreditLimit,
            Balance = request.Balance ?? existing.Balance,
            Status = request.Status ?? existing.Status,
            UpdatedAt = DateTimeOffset.UtcNow,
            Extra = mergedExtra,
        };
        AdminCrudStore<BusinessAccountResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}

public sealed class DeleteBusinessAccountCommandHandler
    : IRequestHandler<DeleteBusinessAccountCommand, Result>
{
    public Task<Result> Handle(DeleteBusinessAccountCommand request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        if (!AdminCrudStore<BusinessAccountResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Business account", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}

public sealed class UpdateBusinessAccountStatusCommandHandler
    : IRequestHandler<UpdateBusinessAccountStatusCommand, Result<BusinessAccountResponse>>
{
    public Task<Result<BusinessAccountResponse>> Handle(
        UpdateBusinessAccountStatusCommand request, CancellationToken cancellationToken)
    {
        BusinessAccountSeeds.Ensure();
        if (!AdminCrudStore<BusinessAccountResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<BusinessAccountResponse>(AdminErrors.NotFound("Business account", request.Id)));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Task.FromResult(Result.Failure<BusinessAccountResponse>(
                Error.Validation("admin.status_required", "Status is required.")));
        }

        var updated = existing with { Status = request.Status.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        AdminCrudStore<BusinessAccountResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}
