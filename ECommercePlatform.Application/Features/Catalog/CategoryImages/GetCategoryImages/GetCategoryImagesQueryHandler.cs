using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImages;

public sealed class GetCategoryImagesQueryHandler
    : IQueryHandler<GetCategoryImagesQuery, Result<IReadOnlyCollection<CategoryImageResponse>>>
{
    private readonly ICategoryImageRepository _images;
    private readonly ICategoryRepository _categories;

    public GetCategoryImagesQueryHandler(
        ICategoryImageRepository images, ICategoryRepository categories)
    {
        _images = images;
        _categories = categories;
    }

    public async Task<Result<IReadOnlyCollection<CategoryImageResponse>>> Handle(
        GetCategoryImagesQuery request, CancellationToken cancellationToken)
    {
        // Distinguish "no such category" (404) from "category with no images" (empty list).
        if (!await _categories.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return Result.Failure<IReadOnlyCollection<CategoryImageResponse>>(
                CatalogErrors.CategoryNotFound);
        }

        var images = await _images.GetForCategoryAsync(request.CategoryId, cancellationToken);

        return Result.Success<IReadOnlyCollection<CategoryImageResponse>>(
            images.OrderBy(i => i.DisplayOrder).Select(i => i.ToResponse()).ToArray());
    }
}
