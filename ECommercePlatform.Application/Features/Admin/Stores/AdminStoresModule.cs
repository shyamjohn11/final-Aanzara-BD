using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Stores;

// IDs #100-104 — GET list / GET by id / POST / PUT / DELETE, Admin-role.

public sealed record StoreAdminResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? ContactNumber { get; init; }
    public string? OpeningHours { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetStoresQuery : IQuery<Result<PagedResult<StoreAdminResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetStoreByIdQuery(Guid Id) : IQuery<Result<StoreAdminResponse>>;

public sealed record CreateStoreCommand : ICommand<Result<StoreAdminResponse>>
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(30)] public string? ContactNumber { get; init; }
    [MaxLength(200)] public string? OpeningHours { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateStoreCommand : ICommand<Result<StoreAdminResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(30)] public string? ContactNumber { get; init; }
    [MaxLength(200)] public string? OpeningHours { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteStoreCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateStoreStatusCommand(Guid Id, string Status) : ICommand<Result<StoreAdminResponse>>;

internal static class StoreAdminSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<StoreAdminResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            return new List<StoreAdminResponse>
            {
                new() { Id = Guid.NewGuid(), Name = "Koramangala Store", Address = "80 Feet Rd, Bengaluru", Phone = "9876543210", ContactNumber = "9876543210", OpeningHours = "9am-9pm", Latitude = 12.9352m, Longitude = 77.6245m, Status = "Active", CreatedAt = now.AddDays(-90), UpdatedAt = now },
                new() { Id = Guid.NewGuid(), Name = "HSR Layout Store", Address = "27th Main, Bengaluru", Phone = "9876543211", ContactNumber = "9876543211", OpeningHours = "9am-10pm", Latitude = 12.9116m, Longitude = 77.6474m, Status = "Active", CreatedAt = now.AddDays(-60), UpdatedAt = now },
            };
        });
    }
}

public sealed class GetStoresQueryHandler
    : IQueryHandler<GetStoresQuery, Result<PagedResult<StoreAdminResponse>>>
{
    public Task<Result<PagedResult<StoreAdminResponse>>> Handle(
        GetStoresQuery request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        var filtered = AdminCrudStore<StoreAdminResponse>.All()
            .Where(s => AdminPaging.MatchesStatus(request.Status, s.Status))
            .Where(s => AdminPaging.Matches(request.Search, s.Name, s.Address, s.Phone, s.ContactNumber)
                || AdminPaging.MatchesExtra(request.Search, s.Extra))
            .OrderBy(s => s.Name);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class GetStoreByIdQueryHandler
    : IQueryHandler<GetStoreByIdQuery, Result<StoreAdminResponse>>
{
    public Task<Result<StoreAdminResponse>> Handle(
        GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        return Task.FromResult(
            AdminCrudStore<StoreAdminResponse>.TryGet(request.Id, out var v) && v is not null
                ? Result.Success(v)
                : Result.Failure<StoreAdminResponse>(AdminErrors.NotFound("Store", request.Id)));
    }
}

public sealed class CreateStoreCommandHandler
    : IRequestHandler<CreateStoreCommand, Result<StoreAdminResponse>>
{
    public Task<Result<StoreAdminResponse>> Handle(
        CreateStoreCommand request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult(Result.Failure<StoreAdminResponse>(
                Error.Validation("admin.store_name_required", "Store name is required.")));
        }

        var now = DateTimeOffset.UtcNow;
        var response = new StoreAdminResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            Address = request.Address,
            Phone = request.Phone ?? request.ContactNumber,
            ContactNumber = request.ContactNumber ?? request.Phone,
            OpeningHours = request.OpeningHours,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!,
            CreatedAt = now,
            UpdatedAt = now,
            Extra = request.Extra,
        };
        AdminCrudStore<StoreAdminResponse>.Put(response);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class UpdateStoreCommandHandler
    : IRequestHandler<UpdateStoreCommand, Result<StoreAdminResponse>>
{
    public Task<Result<StoreAdminResponse>> Handle(
        UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        if (!AdminCrudStore<StoreAdminResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<StoreAdminResponse>(AdminErrors.NotFound("Store", request.Id)));
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
            Address = request.Address ?? existing.Address,
            Phone = request.Phone ?? request.ContactNumber ?? existing.Phone,
            ContactNumber = request.ContactNumber ?? request.Phone ?? existing.ContactNumber,
            OpeningHours = request.OpeningHours ?? existing.OpeningHours,
            Latitude = request.Latitude ?? existing.Latitude,
            Longitude = request.Longitude ?? existing.Longitude,
            Status = request.Status ?? existing.Status,
            UpdatedAt = DateTimeOffset.UtcNow,
            Extra = mergedExtra,
        };
        AdminCrudStore<StoreAdminResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}

public sealed class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand, Result>
{
    public Task<Result> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        if (!AdminCrudStore<StoreAdminResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Store", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}

public sealed class UpdateStoreStatusCommandHandler
    : IRequestHandler<UpdateStoreStatusCommand, Result<StoreAdminResponse>>
{
    public Task<Result<StoreAdminResponse>> Handle(
        UpdateStoreStatusCommand request, CancellationToken cancellationToken)
    {
        StoreAdminSeeds.Ensure();
        if (!AdminCrudStore<StoreAdminResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<StoreAdminResponse>(AdminErrors.NotFound("Store", request.Id)));
        }

        var updated = existing with
        {
            Status = request.Status,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        AdminCrudStore<StoreAdminResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}
