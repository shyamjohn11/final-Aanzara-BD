using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDetailResponse>>
{
    private readonly ICategoryRepository _categories;

    public GetCategoryByIdQueryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<CategoryDetailResponse>> Handle(
        GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetWithImagesAsync(request.CategoryId, cancellationToken);

        return category is null
            ? Result.Failure<CategoryDetailResponse>(CatalogErrors.CategoryNotFound)
            : Result.Success(category.ToDetailResponse(category.Images));
    }
}
