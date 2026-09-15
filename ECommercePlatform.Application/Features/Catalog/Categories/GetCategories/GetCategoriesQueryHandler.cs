using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IQueryHandler<GetCategoriesQuery, Result<PagedResult<CategoryResponse>>>
{
    private readonly ICategoryRepository _categories;
    private readonly ICategoryImageRepository _images;

    public GetCategoriesQueryHandler(
        ICategoryRepository categories, ICategoryImageRepository images)
    {
        _categories = categories;
        _images = images;
    }

    public async Task<Result<PagedResult<CategoryResponse>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await _categories.SearchAsync(
            request.Search?.Trim(), request.IsActive, request.Page, request.PageSize, cancellationToken);

        // Attach each category's primary image as a servable streaming URL
        // (stored URLs are not directly servable). No image row → no URL.
        var categoryIds = page.Items.Select(c => c.CategoryId).ToArray();
        IReadOnlyList<CategoryImage> images = categoryIds.Length == 0
            ? []
            : await _images.GetForCategoriesAsync(categoryIds, cancellationToken);

        var primaryByCategory = images
            .GroupBy(i => i.CategoryId)
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault(i => i.IsPrimary) ?? g.First());

        var items = page.Items
            .Select(c => primaryByCategory.TryGetValue(c.CategoryId, out var image)
                ? c.ToResponse() with
                {
                    PrimaryImageUrl = $"/api/v1/categories/{c.CategoryId}/image/file"
                }
                : c.ToResponse())
            .ToArray();

        return Result.Success(new PagedResult<CategoryResponse>(
            items,
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
