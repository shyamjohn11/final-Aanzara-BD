using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.BrandImages.GetBrandImageFile;

public sealed class GetBrandImageFileQueryHandler
    : IQueryHandler<GetBrandImageFileQuery, Result<CategoryImageFileResponse>>
{
    private readonly IBrandRepository _brands;
    private readonly IFileStorageService _storage;

    public GetBrandImageFileQueryHandler(IBrandRepository brands, IFileStorageService storage)
    {
        _brands = brands;
        _storage = storage;
    }

    public async Task<Result<CategoryImageFileResponse>> Handle(
        GetBrandImageFileQuery request, CancellationToken cancellationToken)
    {
        var brand = await _brands.GetWithImagesAsync(request.BrandId, cancellationToken);

        if (brand is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.BrandNotFound);
        }

        // Mirror the category/sub-category behavior: primary first, then any image.
        var image = brand.Images?.FirstOrDefault(i => i.IsPrimary)
            ?? brand.Images?.FirstOrDefault();

        if (image is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageFileMissing);
        }

        var path = _storage.TryResolvePath(image.ImageUrl);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageFileMissing);
        }

        return Result.Success(new CategoryImageFileResponse(
            path,
            GetContentType(image.ImageUrl),
            GetFileName(image.ImageUrl)));
    }

    private static string GetContentType(string url)
    {
        var extension = Path.GetExtension(url).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    private static string GetFileName(string url)
        => url.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "image";
}
