using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImageFile;

public sealed class GetSubCategoryImageFileQueryHandler
    : IQueryHandler<GetSubCategoryImageFileQuery, Result<CategoryImageFileResponse>>
{
    private readonly ISubCategoryRepository _subCategories;
    private readonly ISubCategoryImageRepository _images;
    private readonly IFileStorageService _storage;

    public GetSubCategoryImageFileQueryHandler(
        ISubCategoryRepository subCategories,
        ISubCategoryImageRepository images,
        IFileStorageService storage)
    {
        _subCategories = subCategories;
        _images = images;
        _storage = storage;
    }

    public async Task<Result<CategoryImageFileResponse>> Handle(
        GetSubCategoryImageFileQuery request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategories.GetByIdAsync(
            request.SubCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.SubCategoryNotFound);
        }

        var images = await _images.GetForSubCategoryAsync(request.SubCategoryId, cancellationToken);

        // Mirror the category behavior: primary first, then lowest display order.
        var image = images.FirstOrDefault(i => i.IsPrimary)
            ?? images.OrderBy(i => i.DisplayOrder).FirstOrDefault();

        if (image is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.SubCategoryHasNoImages);
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
            ".jpg" or ".jpeg" or ".jfif" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    private static string GetFileName(string url)
        => url.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "image";
}
