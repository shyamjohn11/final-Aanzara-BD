using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.StoreOffers;

// IDs #105-109 — GET list / GET by id / POST / PUT / DELETE, Admin-role.
// Persisted in SQL Server (StoreOffers table). Wire shape unchanged.

public sealed record StoreOfferResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DiscountValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetStoreOffersQuery : IQuery<Result<PagedResult<StoreOfferResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? StoreId { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetStoreOfferByIdQuery(Guid Id) : IQuery<Result<StoreOfferResponse>>;

public sealed record CreateStoreOfferCommand : ICommand<Result<StoreOfferResponse>>
{
    public Guid? StoreId { get; init; }
    [MaxLength(200)] public string? StoreName { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [Range(0, 1000000)] public decimal DiscountValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateStoreOfferCommand : ICommand<Result<StoreOfferResponse>>
{
    public Guid Id { get; init; }
    public Guid? StoreId { get; init; }
    [MaxLength(200)] public string? StoreName { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    public decimal? DiscountValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteStoreOfferCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateStoreOfferStatusCommand(Guid Id, string Status) : ICommand<Result<StoreOfferResponse>>;

internal static class StoreOfferMappings
{
    internal static StoreOfferResponse ToDto(this StoreOffer offer) => new()
    {
        Id = offer.Id,
        StoreId = offer.StoreId,
        StoreName = offer.StoreName,
        Title = offer.Title,
        Description = offer.Description,
        DiscountValue = offer.DiscountValue,
        StartsAt = offer.StartsAt,
        EndsAt = offer.EndsAt,
        Status = offer.Status,
        CreatedAt = offer.CreatedAt,
        UpdatedAt = offer.UpdatedAt
    };
}

public sealed class GetStoreOffersQueryHandler
    : IQueryHandler<GetStoreOffersQuery, Result<PagedResult<StoreOfferResponse>>>
{
    private readonly IAdminRepository<StoreOffer> _offers;

    public GetStoreOffersQueryHandler(IAdminRepository<StoreOffer> offers) => _offers = offers;

    public async Task<Result<PagedResult<StoreOfferResponse>>> Handle(
        GetStoreOffersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<StoreOffer, bool>> filter = AdminFilters.True<StoreOffer>();

        if (request.StoreId.HasValue)
        {
            var storeId = request.StoreId.Value;
            filter = filter.And(o => o.StoreId == storeId);
        }

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(o => o.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(o =>
                o.Title.Contains(s)
                || o.StoreName.Contains(s)
                || (o.Description != null && o.Description.Contains(s)));
        }

        var total = await _offers.CountAsync(filter, cancellationToken);
        var items = await _offers.PageAsync(
            filter,
            q => q.OrderByDescending(o => o.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<StoreOfferResponse>(
            items.Select(o => o.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetStoreOfferByIdQueryHandler
    : IQueryHandler<GetStoreOfferByIdQuery, Result<StoreOfferResponse>>
{
    private readonly IAdminRepository<StoreOffer> _offers;

    public GetStoreOfferByIdQueryHandler(IAdminRepository<StoreOffer> offers) => _offers = offers;

    public async Task<Result<StoreOfferResponse>> Handle(
        GetStoreOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        return offer is null
            ? Result.Failure<StoreOfferResponse>(AdminErrors.NotFound("Store offer", request.Id))
            : Result.Success(offer.ToDto());
    }
}

public sealed class CreateStoreOfferCommandHandler
    : IRequestHandler<CreateStoreOfferCommand, Result<StoreOfferResponse>>
{
    private readonly IAdminRepository<StoreOffer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStoreOfferCommandHandler(IAdminRepository<StoreOffer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StoreOfferResponse>> Handle(
        CreateStoreOfferCommand request, CancellationToken cancellationToken)
    {
        var title = request.Title ?? request.Name ?? "Untitled store offer";
        var offer = new StoreOffer
        {
            Id = Guid.NewGuid(),
            StoreId = request.StoreId ?? Guid.NewGuid(),
            StoreName = request.StoreName ?? "Store",
            Title = title,
            Description = request.Description,
            DiscountValue = request.DiscountValue,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!
        };

        _offers.Add(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.ToDto());
    }
}

public sealed class UpdateStoreOfferCommandHandler
    : IRequestHandler<UpdateStoreOfferCommand, Result<StoreOfferResponse>>
{
    private readonly IAdminRepository<StoreOffer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStoreOfferCommandHandler(IAdminRepository<StoreOffer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StoreOfferResponse>> Handle(
        UpdateStoreOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        if (offer is null)
        {
            return Result.Failure<StoreOfferResponse>(AdminErrors.NotFound("Store offer", request.Id));
        }

        offer.StoreId = request.StoreId ?? offer.StoreId;
        offer.StoreName = request.StoreName ?? offer.StoreName;
        offer.Title = request.Title ?? request.Name ?? offer.Title;
        offer.Description = request.Description ?? offer.Description;
        offer.DiscountValue = request.DiscountValue ?? offer.DiscountValue;
        offer.StartsAt = request.StartsAt ?? offer.StartsAt;
        offer.EndsAt = request.EndsAt ?? offer.EndsAt;
        offer.Status = request.Status ?? offer.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.ToDto());
    }
}

public sealed class DeleteStoreOfferCommandHandler : IRequestHandler<DeleteStoreOfferCommand, Result>
{
    private readonly IAdminRepository<StoreOffer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStoreOfferCommandHandler(IAdminRepository<StoreOffer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteStoreOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        if (offer is null)
        {
            return Result.Failure(AdminErrors.NotFound("Store offer", request.Id));
        }

        _offers.Remove(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateStoreOfferStatusCommandHandler
    : IRequestHandler<UpdateStoreOfferStatusCommand, Result<StoreOfferResponse>>
{
    private readonly IAdminRepository<StoreOffer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStoreOfferStatusCommandHandler(IAdminRepository<StoreOffer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StoreOfferResponse>> Handle(
        UpdateStoreOfferStatusCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        if (offer is null)
        {
            return Result.Failure<StoreOfferResponse>(AdminErrors.NotFound("Store offer", request.Id));
        }

        offer.Status = request.Status;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.ToDto());
    }
}
