using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFileById;

public sealed class GetCategoryImageFileByIdQueryHandler
    : IQueryHandler<GetCategoryImageFileByIdQuery, Result<CategoryImageFileResponse>>
{
    private readonly ICategoryImageRepository _images;
    private readonly IFileStorageService _storage;

    public GetCategoryImageFileByIdQueryHandler(
        ICategoryImageRepository images, IFileStorageService storage)
    {
        _images = images;
        _storage = storage;
    }

    public async Task<Result<CategoryImageFileResponse>> Handle(
        GetCategoryImageFileByIdQuery request, CancellationToken cancellationToken)
    {
        var image = await _images.GetByIdAsync(request.CategoryImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageNotFound);
        }

        var path = _storage.TryResolvePath(image.ImageUrl);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.ImageFileMissing);
        }

        return Result.Success(new CategoryImageFileResponse(
            path,
            string.IsNullOrWhiteSpace(image.ContentType)
                ? GetContentType(image.ImageUrl)
                : image.ContentType,
            string.IsNullOrWhiteSpace(image.FileName)
                ? GetFileName(image.ImageUrl)
                : image.FileName));
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
