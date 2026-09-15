using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFile;

public sealed class GetCategoryImageFileQueryHandler
    : IQueryHandler<GetCategoryImageFileQuery, Result<CategoryImageFileResponse>>
{
    private readonly ICategoryImageRepository _images;
    private readonly ICategoryRepository _categories;

    public GetCategoryImageFileQueryHandler(
        ICategoryImageRepository images, ICategoryRepository categories)
    {
        _images = images;
        _categories = categories;
    }

    public async Task<Result<CategoryImageFileResponse>> Handle(
        GetCategoryImageFileQuery request, CancellationToken cancellationToken)
    {
        // Distinguish "no such category" (404, category_not_found) from
        // "category exists but has no images" (404, category_has_no_images).
        if (!await _categories.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.CategoryNotFound);
        }

        var images = await _images.GetForCategoryAsync(request.CategoryId, cancellationToken);

        // Primary wins; if none is flagged, fall back to the first by display order.
        var image = images.FirstOrDefault(i => i.IsPrimary)
            ?? images.OrderBy(i => i.DisplayOrder).FirstOrDefault();

        if (image is null)
        {
            return Result.Failure<CategoryImageFileResponse>(CatalogErrors.CategoryHasNoImages);
        }

        return Result.Success(new CategoryImageFileResponse(
            image.FilePath, image.ContentType, image.FileName));
    }
}