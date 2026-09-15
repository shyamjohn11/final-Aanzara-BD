using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Combos;

// IDs #90-94 — GET list / GET by id / POST / PUT / DELETE, Admin-role.
// Persisted in SQL Server (Combos table). Wire shape unchanged.

public sealed record ComboResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<Guid> ProductIds { get; init; } = new();
    public decimal Price { get; init; }
    public decimal OriginalPrice { get; init; }
    public string? ImageUrl { get; init; }
    public string Status { get; init; } = "Active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetCombosQuery : IQuery<Result<PagedResult<ComboResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetComboByIdQuery(Guid Id) : IQuery<Result<ComboResponse>>;

public sealed record CreateComboCommand : ICommand<Result<ComboResponse>>
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    public List<Guid>? ProductIds { get; init; }
    [Range(0, 10000000)] public decimal Price { get; init; }
    [Range(0, 10000000)] public decimal OriginalPrice { get; init; }
    [MaxLength(1000)] public string? ImageUrl { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    /// <summary>Set from the multipart file field; never bound from JSON.</summary>
    public FileUpload? ImageFile { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateComboCommand : ICommand<Result<ComboResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    public List<Guid>? ProductIds { get; init; }
    public decimal? Price { get; init; }
    public decimal? OriginalPrice { get; init; }
    [MaxLength(1000)] public string? ImageUrl { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    /// <summary>New file: replaces the stored image, old file is deleted.</summary>
    public FileUpload? ImageFile { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteComboCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateComboStatusCommand(Guid Id, string Status) : ICommand<Result<ComboResponse>>;

internal static class ComboMappings
{
    internal static readonly string[] AllowedImageTypes =
        ["image/png", "image/jpeg", "image/webp"];

    internal const long MaxImageBytes = 2 * 1024 * 1024;

    internal const string ImageSubFolder = "Combos";

    internal static ComboResponse ToDto(this Combo combo) => new()
    {
        Id = combo.Id,
        Name = combo.Name,
        Title = combo.Title,
        Description = combo.Description,
        ProductIds = combo.ProductIds,
        Price = combo.Price,
        OriginalPrice = combo.OriginalPrice,
        ImageUrl = combo.ImageUrl,
        Status = combo.Status,
        CreatedAt = combo.CreatedAt,
        UpdatedAt = combo.UpdatedAt
    };
}

public sealed class GetCombosQueryHandler
    : IQueryHandler<GetCombosQuery, Result<PagedResult<ComboResponse>>>
{
    private readonly IAdminRepository<Combo> _combos;

    public GetCombosQueryHandler(IAdminRepository<Combo> combos) => _combos = combos;

    public async Task<Result<PagedResult<ComboResponse>>> Handle(
        GetCombosQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Combo, bool>> filter = AdminFilters.True<Combo>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(c => c.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(c =>
                c.Name.Contains(s)
                || c.Title.Contains(s)
                || (c.Description != null && c.Description.Contains(s)));
        }

        var total = await _combos.CountAsync(filter, cancellationToken);
        var items = await _combos.PageAsync(
            filter,
            q => q.OrderByDescending(c => c.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<ComboResponse>(
            items.Select(c => c.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetComboByIdQueryHandler
    : IQueryHandler<GetComboByIdQuery, Result<ComboResponse>>
{
    private readonly IAdminRepository<Combo> _combos;

    public GetComboByIdQueryHandler(IAdminRepository<Combo> combos) => _combos = combos;

    public async Task<Result<ComboResponse>> Handle(
        GetComboByIdQuery request, CancellationToken cancellationToken)
    {
        var combo = await _combos.GetByIdAsync(request.Id, cancellationToken);

        return combo is null
            ? Result.Failure<ComboResponse>(AdminErrors.NotFound("Combo", request.Id))
            : Result.Success(combo.ToDto());
    }
}

public sealed class CreateComboCommandHandler
    : IRequestHandler<CreateComboCommand, Result<ComboResponse>>
{
    private readonly IAdminRepository<Combo> _combos;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public CreateComboCommandHandler(
        IAdminRepository<Combo> combos,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _combos = combos;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComboResponse>> Handle(
        CreateComboCommand request, CancellationToken cancellationToken)
    {
        var imageError = ValidateImage(request.ImageFile);
        if (imageError is not null)
        {
            return Result.Failure<ComboResponse>(imageError);
        }

        // Multipart file wins; otherwise the supplied URL string is kept.
        string? imageUrl = request.ImageUrl;
        if (request.ImageFile is not null)
        {
            var stored = await _fileStorage.SaveAsync(
                request.ImageFile, ComboMappings.ImageSubFolder, cancellationToken);
            imageUrl = stored.Url;
        }

        var name = request.Name ?? request.Title ?? "Untitled combo";
        var combo = new Combo
        {
            Id = Guid.NewGuid(),
            Name = name,
            Title = request.Title ?? name,
            Description = request.Description,
            ProductIds = request.ProductIds ?? new(),
            Price = request.Price,
            OriginalPrice = request.OriginalPrice,
            ImageUrl = imageUrl,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!
        };

        _combos.Add(combo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(combo.ToDto());
    }

    private static Error? ValidateImage(FileUpload? file)
    {
        if (file is null || file.Length <= 0)
        {
            return null;
        }

        if (!ComboMappings.AllowedImageTypes.Any(t =>
                string.Equals(t, file.ContentType, StringComparison.OrdinalIgnoreCase)))
        {
            return Error.Validation("admin.combo_image_invalid_type",
                "Only image/png, image/jpeg and image/webp are accepted.");
        }

        if (file.Length > ComboMappings.MaxImageBytes)
        {
            return Error.Validation("admin.combo_image_too_large",
                "Image must be 2 MB or smaller.");
        }

        return null;
    }
}

public sealed class UpdateComboCommandHandler
    : IRequestHandler<UpdateComboCommand, Result<ComboResponse>>
{
    private readonly IAdminRepository<Combo> _combos;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComboCommandHandler(
        IAdminRepository<Combo> combos,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _combos = combos;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComboResponse>> Handle(
        UpdateComboCommand request, CancellationToken cancellationToken)
    {
        var combo = await _combos.GetByIdAsync(request.Id, cancellationToken);

        if (combo is null)
        {
            return Result.Failure<ComboResponse>(AdminErrors.NotFound("Combo", request.Id));
        }

        if (request.ImageFile is not null)
        {
            if (!ComboMappings.AllowedImageTypes.Any(t =>
                    string.Equals(t, request.ImageFile.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure<ComboResponse>(Error.Validation("admin.combo_image_invalid_type",
                    "Only image/png, image/jpeg and image/webp are accepted."));
            }

            if (request.ImageFile.Length <= 0 || request.ImageFile.Length > ComboMappings.MaxImageBytes)
            {
                return Result.Failure<ComboResponse>(Error.Validation("admin.combo_image_too_large",
                    "Image must be non-empty and 2 MB or smaller."));
            }
        }

        string? oldImageUrl = null;
        if (request.ImageFile is not null)
        {
            var stored = await _fileStorage.SaveAsync(
                request.ImageFile, ComboMappings.ImageSubFolder, cancellationToken);
            oldImageUrl = combo.ImageUrl;
            combo.ImageUrl = stored.Url;
        }
        else if (request.ImageUrl is not null)
        {
            combo.ImageUrl = request.ImageUrl;
        }

        combo.Name = request.Name ?? request.Title ?? combo.Name;
        combo.Title = request.Title ?? request.Name ?? combo.Title;
        combo.Description = request.Description ?? combo.Description;
        combo.ProductIds = request.ProductIds ?? combo.ProductIds;
        combo.Price = request.Price ?? combo.Price;
        combo.OriginalPrice = request.OriginalPrice ?? combo.OriginalPrice;
        combo.Status = request.Status ?? combo.Status;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (request.ImageFile is not null && combo.ImageUrl is not null)
            {
                await _fileStorage.DeleteByUrlAsync(combo.ImageUrl, cancellationToken);
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl != combo.ImageUrl)
        {
            await _fileStorage.DeleteByUrlAsync(oldImageUrl, cancellationToken);
        }

        return Result.Success(combo.ToDto());
    }
}

public sealed class DeleteComboCommandHandler : IRequestHandler<DeleteComboCommand, Result>
{
    private readonly IAdminRepository<Combo> _combos;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteComboCommandHandler(
        IAdminRepository<Combo> combos,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _combos = combos;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteComboCommand request, CancellationToken cancellationToken)
    {
        var combo = await _combos.GetByIdAsync(request.Id, cancellationToken);

        if (combo is null)
        {
            return Result.Failure(AdminErrors.NotFound("Combo", request.Id));
        }

        var imageUrl = combo.ImageUrl;
        _combos.Remove(combo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            await _fileStorage.DeleteByUrlAsync(imageUrl, cancellationToken);
        }

        return Result.Success();
    }
}

public sealed class UpdateComboStatusCommandHandler
    : IRequestHandler<UpdateComboStatusCommand, Result<ComboResponse>>
{
    private readonly IAdminRepository<Combo> _combos;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComboStatusCommandHandler(IAdminRepository<Combo> combos, IUnitOfWork unitOfWork)
    {
        _combos = combos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComboResponse>> Handle(
        UpdateComboStatusCommand request, CancellationToken cancellationToken)
    {
        var combo = await _combos.GetByIdAsync(request.Id, cancellationToken);

        if (combo is null)
        {
            return Result.Failure<ComboResponse>(AdminErrors.NotFound("Combo", request.Id));
        }

        combo.Status = request.Status;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(combo.ToDto());
    }
}
