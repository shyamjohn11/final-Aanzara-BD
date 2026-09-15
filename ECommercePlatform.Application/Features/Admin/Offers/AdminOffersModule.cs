using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Offers;

// IDs #80-84 — GET list / GET by id / POST / PUT / DELETE, Admin-role.
// Persisted in SQL Server (Offers table). Wire shape unchanged.

public sealed record OfferResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Code { get; init; }
    public string DiscountType { get; init; } = "Percent";
    public decimal DiscountValue { get; init; }
    public decimal MinOrderValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetOffersQuery : IQuery<Result<PagedResult<OfferResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetOfferByIdQuery(Guid Id) : IQuery<Result<OfferResponse>>;

public sealed record CreateOfferCommand : ICommand<Result<OfferResponse>>
{
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(50)] public string? Code { get; init; }
    [MaxLength(20)] public string? DiscountType { get; init; }
    [Range(0, 1000000)] public decimal DiscountValue { get; init; }
    [Range(0, 1000000)] public decimal MinOrderValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateOfferCommand : ICommand<Result<OfferResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    [MaxLength(50)] public string? Code { get; init; }
    [MaxLength(20)] public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal? MinOrderValue { get; init; }
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteOfferCommand(Guid Id) : ICommand<Result>;

internal static class OfferMappings
{
    internal static OfferResponse ToDto(this Offer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Name = offer.Name,
        Description = offer.Description,
        Code = offer.Code,
        DiscountType = offer.DiscountType,
        DiscountValue = offer.DiscountValue,
        MinOrderValue = offer.MinOrderValue,
        StartsAt = offer.StartsAt,
        EndsAt = offer.EndsAt,
        Status = offer.Status,
        CreatedAt = offer.CreatedAt,
        UpdatedAt = offer.UpdatedAt
    };
}

public sealed class GetOffersQueryHandler
    : IQueryHandler<GetOffersQuery, Result<PagedResult<OfferResponse>>>
{
    private readonly IAdminRepository<Offer> _offers;

    public GetOffersQueryHandler(IAdminRepository<Offer> offers) => _offers = offers;

    public async Task<Result<PagedResult<OfferResponse>>> Handle(
        GetOffersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Offer, bool>> filter = AdminFilters.True<Offer>();

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
                || (o.Name != null && o.Name.Contains(s))
                || (o.Description != null && o.Description.Contains(s))
                || (o.Code != null && o.Code.Contains(s)));
        }

        var total = await _offers.CountAsync(filter, cancellationToken);
        var items = await _offers.PageAsync(
            filter,
            q => q.OrderByDescending(o => o.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<OfferResponse>(
            items.Select(o => o.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetOfferByIdQueryHandler
    : IQueryHandler<GetOfferByIdQuery, Result<OfferResponse>>
{
    private readonly IAdminRepository<Offer> _offers;

    public GetOfferByIdQueryHandler(IAdminRepository<Offer> offers) => _offers = offers;

    public async Task<Result<OfferResponse>> Handle(
        GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        return offer is null
            ? Result.Failure<OfferResponse>(AdminErrors.NotFound("Offer", request.Id))
            : Result.Success(offer.ToDto());
    }
}

public sealed class CreateOfferCommandHandler
    : IRequestHandler<CreateOfferCommand, Result<OfferResponse>>
{
    private readonly IAdminRepository<Offer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOfferCommandHandler(IAdminRepository<Offer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferResponse>> Handle(
        CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var title = request.Title ?? request.Name ?? request.Code ?? "Untitled offer";
        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            Title = title,
            Name = request.Name ?? title,
            Description = request.Description,
            Code = request.Code,
            DiscountType = string.IsNullOrWhiteSpace(request.DiscountType) ? "Percent" : request.DiscountType!,
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!
        };

        _offers.Add(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.ToDto());
    }
}

public sealed class UpdateOfferCommandHandler
    : IRequestHandler<UpdateOfferCommand, Result<OfferResponse>>
{
    private readonly IAdminRepository<Offer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOfferCommandHandler(IAdminRepository<Offer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferResponse>> Handle(
        UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        if (offer is null)
        {
            return Result.Failure<OfferResponse>(AdminErrors.NotFound("Offer", request.Id));
        }

        offer.Title = request.Title ?? request.Name ?? offer.Title;
        offer.Name = request.Name ?? request.Title ?? offer.Name;
        offer.Description = request.Description ?? offer.Description;
        offer.Code = request.Code ?? offer.Code;
        offer.DiscountType = request.DiscountType ?? offer.DiscountType;
        offer.DiscountValue = request.DiscountValue ?? offer.DiscountValue;
        offer.MinOrderValue = request.MinOrderValue ?? offer.MinOrderValue;
        offer.StartsAt = request.StartsAt ?? offer.StartsAt;
        offer.EndsAt = request.EndsAt ?? offer.EndsAt;
        offer.Status = request.Status ?? offer.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.ToDto());
    }
}

public sealed class DeleteOfferCommandHandler : IRequestHandler<DeleteOfferCommand, Result>
{
    private readonly IAdminRepository<Offer> _offers;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOfferCommandHandler(IAdminRepository<Offer> offers, IUnitOfWork unitOfWork)
    {
        _offers = offers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offers.GetByIdAsync(request.Id, cancellationToken);

        if (offer is null)
        {
            return Result.Failure(AdminErrors.NotFound("Offer", request.Id));
        }

        _offers.Remove(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
