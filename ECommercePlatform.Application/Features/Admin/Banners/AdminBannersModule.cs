using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Banners;

// IDs #75-79 + PATCH …/{id}/status. Persisted in SQL Server (Banners table).
// Wire shape is unchanged: bannerId, title, subtitle, imageUrl, link,
// position, status, startDate/endDate (yyyy-MM-dd), clicks. Admin-role.

public sealed record BannerResponse : IAdminEntity
{
    [JsonPropertyName("bannerId")]
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? ImageUrl { get; init; }
    public string? Link { get; init; }
    public string Position { get; init; } = BannerRules.DefaultPosition;
    public string Status { get; init; } = "Active";
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public int Clicks { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>Server mirror of the client-side banner rules from the spec.</summary>
public static class BannerRules
{
    public static readonly string[] Positions =
    [
        "Home Hero", "Home Secondary", "Offers Banner", "Business Page", "Category Banner"
    ];

    public static readonly string[] Statuses = ["Active", "Scheduled", "Inactive"];

    public const string DefaultPosition = "Home Hero";

    public static readonly string[] AllowedContentTypes =
        ["image/png", "image/jpeg", "image/webp"];

    public const long MaxFileBytes = 2 * 1024 * 1024;

    public const string DateFormat = "yyyy-MM-dd";

    public const string ImageSubFolder = "Banners";
}

public sealed record GetBannersQuery : IQuery<Result<PagedResult<BannerResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetBannerByIdQuery(Guid Id) : IQuery<Result<BannerResponse>>;

public sealed record CreateBannerCommand : ICommand<Result<BannerResponse>>
{
    [Required]
    [MinLength(3)]
    [MaxLength(80)]
    public string? Title { get; init; }

    [MaxLength(150)]
    public string? Subtitle { get; init; }

    public string? Link { get; init; }

    [Required]
    public string? Position { get; init; }

    [Required]
    public string? Status { get; init; }

    [Required]
    public string? StartDate { get; init; }

    [Required]
    public string? EndDate { get; init; }

    /// <summary>Set from the multipart file field; never bound from JSON.</summary>
    public FileUpload? ImageFile { get; init; }

    /// <summary>JSON path: the existing URL string sent as "image".</summary>
    [JsonPropertyName("image")]
    public string? ImageUrl { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateBannerCommand : ICommand<Result<BannerResponse>>
{
    public Guid Id { get; init; }

    [MinLength(3)]
    [MaxLength(80)]
    public string? Title { get; init; }

    [MaxLength(150)]
    public string? Subtitle { get; init; }

    public string? Link { get; init; }
    public string? Position { get; init; }
    public string? Status { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }

    /// <summary>New file: replaces the stored image, old file is deleted.</summary>
    public FileUpload? ImageFile { get; init; }

    /// <summary>JSON path: the existing URL string sent as "image" (kept as-is).</summary>
    [JsonPropertyName("image")]
    public string? ImageUrl { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteBannerCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateBannerStatusCommand(Guid Id, string Status)
    : ICommand<Result<BannerResponse>>;

internal static class BannerValidation
{
    internal static Error? Position(string? position) =>
        string.IsNullOrWhiteSpace(position) || BannerRules.Positions.Any(p =>
            string.Equals(p, position.Trim(), StringComparison.OrdinalIgnoreCase))
            ? null
            : Error.Validation("admin.banner_position_invalid",
                $"Position must be one of: {string.Join(", ", BannerRules.Positions)}.");

    internal static Error? Status(string? status) =>
        string.IsNullOrWhiteSpace(status) || BannerRules.Statuses.Any(s =>
            string.Equals(s, status.Trim(), StringComparison.OrdinalIgnoreCase))
            ? null
            : Error.Validation("admin.banner_status_invalid",
                $"Status must be one of: {string.Join(", ", BannerRules.Statuses)}.");

    internal static Error? Link(string? link) =>
        string.IsNullOrWhiteSpace(link)
        || link.StartsWith("/", StringComparison.Ordinal)
        || link.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || link.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? null
            : Error.Validation("admin.banner_link_invalid",
                "Link must be an internal path (/…) or an http(s):// URL.");

    internal static Error? DateRange(string? startDate, string? endDate, out string? start, out string? end)
    {
        start = null;
        end = null;

        if (startDate is null || endDate is null)
        {
            return null;
        }

        if (!DateOnly.TryParseExact(startDate.Trim(), BannerRules.DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var startParsed))
        {
            return Error.Validation("admin.banner_startdate_invalid",
                $"StartDate must use {BannerRules.DateFormat}.");
        }

        if (!DateOnly.TryParseExact(endDate.Trim(), BannerRules.DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var endParsed))
        {
            return Error.Validation("admin.banner_enddate_invalid",
                $"EndDate must use {BannerRules.DateFormat}.");
        }

        if (endParsed < startParsed)
        {
            return Error.Validation("admin.banner_daterange_invalid",
                "EndDate must be on or after StartDate.");
        }

        start = startParsed.ToString(BannerRules.DateFormat, CultureInfo.InvariantCulture);
        end = endParsed.ToString(BannerRules.DateFormat, CultureInfo.InvariantCulture);
        return null;
    }

    internal static Error? ImageFile(FileUpload? file, bool required)
    {
        if (file is null)
        {
            return required
                ? Error.Validation("admin.banner_image_required", "An image file is required.")
                : null;
        }

        if (file.Length <= 0)
        {
            return Error.Validation("admin.banner_image_required", "An image file is required.");
        }

        if (!BannerRules.AllowedContentTypes.Any(t =>
                string.Equals(t, file.ContentType, StringComparison.OrdinalIgnoreCase)))
        {
            return Error.Validation("admin.banner_image_invalid_type",
                "Only image/png, image/jpeg and image/webp are accepted.");
        }

        if (file.Length > BannerRules.MaxFileBytes)
        {
            return Error.Validation("admin.banner_image_too_large",
                "Image must be 2 MB or smaller.");
        }

        return null;
    }
}

internal static class BannerMappings
{
    private static string? NormalizeStoredUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        var trimmed = url.Trim();
        // Stored URLs historically used http://192.168.31.9:5000/uploads/... (PublicBaseUrl)
        // — normalize any absolute URL that contains /uploads/ to a same-origin relative path
        // so it resolves via the /uploads static files and the Next.js /uploads/:path* proxy.
        var idx = trimmed.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var path = trimmed[idx..];
            // Ensure leading slash
            return path.StartsWith("/") ? path : "/" + path;
        }
        return trimmed;
    }

    internal static BannerResponse ToDto(this Banner banner) => new()
    {
        Id = banner.Id,
        Title = banner.Title,
        Subtitle = banner.Subtitle,
        ImageUrl = NormalizeStoredUrl(banner.ImageUrl),
        Link = banner.Link,
        Position = banner.Position,
        Status = banner.Status,
        StartDate = banner.StartDate,
        EndDate = banner.EndDate,
        Clicks = banner.Clicks,
        CreatedAt = banner.CreatedAt,
        UpdatedAt = banner.UpdatedAt
    };
}

public sealed class GetBannersQueryHandler
    : IQueryHandler<GetBannersQuery, Result<PagedResult<BannerResponse>>>
{
    private readonly IAdminRepository<Banner> _banners;

    public GetBannersQueryHandler(IAdminRepository<Banner> banners) => _banners = banners;

    public async Task<Result<PagedResult<BannerResponse>>> Handle(
        GetBannersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Banner, bool>> filter = AdminFilters.True<Banner>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(b => b.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(b =>
                b.Title.Contains(s)
                || (b.Subtitle != null && b.Subtitle.Contains(s))
                || b.Position.Contains(s)
                || (b.Link != null && b.Link.Contains(s)));
        }

        var total = await _banners.CountAsync(filter, cancellationToken);
        var items = await _banners.PageAsync(
            filter,
            q => q.OrderByDescending(b => b.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<BannerResponse>(
            items.Select(b => b.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetBannerByIdQueryHandler
    : IQueryHandler<GetBannerByIdQuery, Result<BannerResponse>>
{
    private readonly IAdminRepository<Banner> _banners;

    public GetBannerByIdQueryHandler(IAdminRepository<Banner> banners) => _banners = banners;

    public async Task<Result<BannerResponse>> Handle(
        GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var banner = await _banners.GetByIdAsync(request.Id, cancellationToken);

        return banner is null
            ? Result.Failure<BannerResponse>(AdminErrors.NotFound("Banner", request.Id))
            : Result.Success(banner.ToDto());
    }
}

public sealed class CreateBannerCommandHandler
    : IRequestHandler<CreateBannerCommand, Result<BannerResponse>>
{
    private readonly IAdminRepository<Banner> _banners;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBannerCommandHandler(
        IAdminRepository<Banner> banners,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _banners = banners;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerResponse>> Handle(
        CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var failure = BannerValidation.Position(request.Position)
            ?? BannerValidation.Status(request.Status)
            ?? BannerValidation.Link(request.Link);

        string? start = null;
        string? end = null;
        if (failure is null)
        {
            failure = BannerValidation.DateRange(request.StartDate, request.EndDate, out start, out end);
        }

        failure ??= BannerValidation.ImageFile(request.ImageFile, required: string.IsNullOrWhiteSpace(request.ImageUrl));

        if (failure is not null)
        {
            return Result.Failure<BannerResponse>(failure);
        }

        // Multipart file wins; otherwise the supplied URL string is kept.
        string? imageUrl = request.ImageUrl;
        if (request.ImageFile is not null)
        {
            var stored = await _fileStorage.SaveAsync(
                request.ImageFile, BannerRules.ImageSubFolder, cancellationToken);
            imageUrl = stored.Url;
        }

        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            Subtitle = request.Subtitle?.Trim(),
            ImageUrl = imageUrl,
            Link = request.Link?.Trim(),
            Position = request.Position!.Trim(),
            Status = request.Status!.Trim(),
            StartDate = start,
            EndDate = end,
            Clicks = 0
        };

        _banners.Add(banner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(banner.ToDto());
    }
}

public sealed class UpdateBannerCommandHandler
    : IRequestHandler<UpdateBannerCommand, Result<BannerResponse>>
{
    private readonly IAdminRepository<Banner> _banners;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBannerCommandHandler(
        IAdminRepository<Banner> banners,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _banners = banners;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerResponse>> Handle(
        UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner is null)
        {
            return Result.Failure<BannerResponse>(AdminErrors.NotFound("Banner", request.Id));
        }

        string? start = banner.StartDate;
        string? end = banner.EndDate;
        Error? failure = null;

        if (request.Position is not null)
        {
            failure ??= BannerValidation.Position(request.Position);
        }

        if (request.Status is not null)
        {
            failure ??= BannerValidation.Status(request.Status);
        }

        if (request.Link is not null)
        {
            failure ??= BannerValidation.Link(request.Link);
        }

        if (request.StartDate is not null || request.EndDate is not null)
        {
            string? parsedStart;
            string? parsedEnd;
            var dateError = BannerValidation.DateRange(
                request.StartDate ?? banner.StartDate,
                request.EndDate ?? banner.EndDate,
                out parsedStart, out parsedEnd);

            if (dateError is not null)
            {
                failure ??= dateError;
            }
            else
            {
                start = parsedStart;
                end = parsedEnd;
            }
        }

        failure ??= BannerValidation.ImageFile(request.ImageFile, required: false);

        if (failure is not null)
        {
            return Result.Failure<BannerResponse>(failure);
        }

        // New file replaces the stored image (old file deleted); an "image"
        // URL string is persisted as-is; otherwise the old image is kept.
        string? oldImageUrl = null;
        if (request.ImageFile is not null)
        {
            var stored = await _fileStorage.SaveAsync(
                request.ImageFile, BannerRules.ImageSubFolder, cancellationToken);
            oldImageUrl = banner.ImageUrl;
            banner.ImageUrl = stored.Url;
        }
        else if (request.ImageUrl is not null)
        {
            banner.ImageUrl = request.ImageUrl;
        }

        banner.Title = request.Title?.Trim() ?? banner.Title;
        banner.Subtitle = request.Subtitle?.Trim() ?? banner.Subtitle;
        banner.Link = request.Link?.Trim() ?? banner.Link;
        banner.Position = request.Position?.Trim() ?? banner.Position;
        banner.Status = request.Status?.Trim() ?? banner.Status;
        banner.StartDate = start;
        banner.EndDate = end;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (request.ImageFile is not null && banner.ImageUrl is not null)
            {
                await _fileStorage.DeleteByUrlAsync(banner.ImageUrl, cancellationToken);
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl != banner.ImageUrl)
        {
            await _fileStorage.DeleteByUrlAsync(oldImageUrl, cancellationToken);
        }

        return Result.Success(banner.ToDto());
    }
}

public sealed class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand, Result>
{
    private readonly IAdminRepository<Banner> _banners;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBannerCommandHandler(
        IAdminRepository<Banner> banners,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _banners = banners;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner is null)
        {
            return Result.Failure(AdminErrors.NotFound("Banner", request.Id));
        }

        var imageUrl = banner.ImageUrl;
        _banners.Remove(banner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            await _fileStorage.DeleteByUrlAsync(imageUrl, cancellationToken);
        }

        return Result.Success();
    }
}

public sealed class UpdateBannerStatusCommandHandler
    : IRequestHandler<UpdateBannerStatusCommand, Result<BannerResponse>>
{
    private readonly IAdminRepository<Banner> _banners;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBannerStatusCommandHandler(
        IAdminRepository<Banner> banners, IUnitOfWork unitOfWork)
    {
        _banners = banners;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerResponse>> Handle(
        UpdateBannerStatusCommand request, CancellationToken cancellationToken)
    {
        var banner = await _banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner is null)
        {
            return Result.Failure<BannerResponse>(AdminErrors.NotFound("Banner", request.Id));
        }

        var failure = BannerValidation.Status(request.Status);
        if (failure is not null || string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<BannerResponse>(
                failure ?? Error.Validation("admin.status_required", "Status is required."));
        }

        banner.Status = request.Status.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(banner.ToDto());
    }
}
