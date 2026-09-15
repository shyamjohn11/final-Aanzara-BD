using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Banners.GetBannerImageFile;

public sealed class GetBannerImageFileQueryHandler
    : IQueryHandler<GetBannerImageFileQuery, Result<BannerImageFileResponse>>
{
    private readonly IAdminRepository<Banner> _banners;
    private readonly IFileStorageService _fileStorage;

    public GetBannerImageFileQueryHandler(
        IAdminRepository<Banner> banners, IFileStorageService fileStorage)
    {
        _banners = banners;
        _fileStorage = fileStorage;
    }

    public async Task<Result<BannerImageFileResponse>> Handle(
        GetBannerImageFileQuery request, CancellationToken cancellationToken)
    {
        var banner = await _banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner is null)
        {
            return Result.Failure<BannerImageFileResponse>(
                AdminErrors.NotFound("Banner", request.Id));
        }

        if (string.IsNullOrWhiteSpace(banner.ImageUrl))
        {
            return Result.Failure<BannerImageFileResponse>(
                Error.NotFound("admin.banner_image_not_found", "This banner has no image."));
        }

        var fullPath = _fileStorage.TryResolvePath(banner.ImageUrl);

        if (fullPath is null)
        {
            return Result.Failure<BannerImageFileResponse>(
                Error.NotFound("admin.banner_image_file_missing",
                    "The image record exists but its file is no longer available."));
        }

        return Result.Success(new BannerImageFileResponse(
            fullPath,
            ContentTypeFromExtension(Path.GetExtension(fullPath)),
            Path.GetFileName(fullPath)));
    }

    private static string ContentTypeFromExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}
