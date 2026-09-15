using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.GetSubCategories;

public sealed class GetSubCategoriesQueryHandler
    : IQueryHandler<GetSubCategoriesQuery, Result<PagedResult<SubCategoryResponse>>>
{
    private readonly ISubCategoryRepository _subCategories;
    private readonly ISubCategoryImageRepository _images;

    public GetSubCategoriesQueryHandler(
        ISubCategoryRepository subCategories, ISubCategoryImageRepository images)
    {
        _subCategories = subCategories;
        _images = images;
    }

    public async Task<Result<PagedResult<SubCategoryResponse>>> Handle(
        GetSubCategoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await _subCategories.SearchAsync(
            request.CategoryId, request.Search?.Trim(), request.IsActive,
            request.Page, request.PageSize, cancellationToken);

        // Attach each sub-category's primary image as a servable streaming URL
        // (stored URLs are not directly servable). No image row → no URL.
        var subCategoryIds = page.Items.Select(s => s.SubCategoryId).ToArray();
        IReadOnlyList<SubCategoryImage> images = subCategoryIds.Length == 0
            ? []
            : await _images.GetForSubCategoriesAsync(subCategoryIds, cancellationToken);

        var primaryBySub = images
            .GroupBy(i => i.SubCategoryId)
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault(i => i.IsPrimary) ?? g.First());

        var items = page.Items
            .Select(s => primaryBySub.TryGetValue(s.SubCategoryId, out var image)
                ? s.ToResponse() with
                {
                    ImageUrl = $"/api/v1/subcategories/{s.SubCategoryId}/image/file"
                }
                : s.ToResponse())
            .ToArray();

        return Result.Success(new PagedResult<SubCategoryResponse>(
            items, page.Page, page.PageSize, page.TotalCount));
    }
}
