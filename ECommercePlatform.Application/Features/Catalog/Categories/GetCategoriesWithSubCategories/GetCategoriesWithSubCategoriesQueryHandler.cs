using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategoriesWithSubCategories;

public sealed class GetCategoriesWithSubCategoriesQueryHandler
    : IQueryHandler<GetCategoriesWithSubCategoriesQuery, Result<IReadOnlyList<CategoryLookupResponse>>>
{
    private readonly ICategoryRepository _categories;

    public GetCategoriesWithSubCategoriesQueryHandler(ICategoryRepository categories)
        => _categories = categories;

    public async Task<Result<IReadOnlyList<CategoryLookupResponse>>> Handle(
        GetCategoriesWithSubCategoriesQuery request, CancellationToken cancellationToken)
    {
        var items = await _categories.GetCategoriesWithSubCategoriesAsync(cancellationToken);

        var response = items
            .Select(i => new CategoryLookupResponse
            {
                CategoryId = i.CategoryId,
                CategoryName = i.CategoryName
            })
            .ToArray();

        return Result.Success<IReadOnlyList<CategoryLookupResponse>>(response);
    }
}