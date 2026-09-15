using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetProductTree;

public sealed class GetProductTreeQueryHandler
    : IQueryHandler<GetProductTreeQuery, Result<ProductTreeResponse>>
{
    private readonly ICategoryRepository _categories;

    public GetProductTreeQueryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<ProductTreeResponse>> Handle(
        GetProductTreeQuery request, CancellationToken cancellationToken)
    {
        // The repository returns the tree already flattened and counted by the
        // database; this handler only reshapes it. Building it here instead would
        // mean one query per category.
        var nodes = await _categories.GetTreeAsync(request.ActiveOnly, cancellationToken);

        var categories = nodes.Select(node => new CategoryTreeResponse
        {
            CategoryId = node.CategoryId,
            CategoryCode = node.CategoryCode,
            CategoryName = node.CategoryName,
            IsActive = node.IsActive,
            // Stored URLs point at a host the browser can't reach; expose the
            // anonymous streaming endpoint instead.
            PrimaryImageUrl = string.IsNullOrEmpty(node.PrimaryImageUrl)
                ? null
                : $"/api/v1/categories/{node.CategoryId}/image/file",
            ProductCount = node.SubCategories.Sum(s => s.ProductCount),
            SubCategories = node.SubCategories.Select(sub => new SubCategoryTreeResponse
            {
                SubCategoryId = sub.SubCategoryId,
                SubCategoryCode = sub.SubCategoryCode,
                SubCategoryName = sub.SubCategoryName,
                IsActive = sub.IsActive,
                PrimaryImageUrl = string.IsNullOrEmpty(sub.PrimaryImageUrl)
                    ? null
                    : $"/api/v1/subcategories/{sub.SubCategoryId}/image/file",
                ProductCount = sub.ProductCount
            }).ToArray()
        }).ToArray();

        return Result.Success(new ProductTreeResponse
        {
            Categories = categories,
            TotalCategories = categories.Length,
            TotalSubCategories = categories.Sum(c => c.SubCategories.Count),
            TotalProducts = categories.Sum(c => c.ProductCount)
        });
    }
}
