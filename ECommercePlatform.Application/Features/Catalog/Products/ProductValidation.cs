using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products;

/// <summary>
/// Cross-field and cross-entity checks that data annotations cannot express.
/// Shared by create and update so both enforce the same invariants.
/// </summary>
internal static class ProductValidation
{
    public static Error? CheckShape(ProductWriteModel model)
        => ProductStatus.IsValid(model.Status) ? null : CatalogErrors.InvalidStatus(model.Status);

    /// <summary>
    /// When a category and/or sub-category is supplied, confirms it exists and,
    /// if both are supplied, that the sub-category hangs off that category.
    /// </summary>
    public static async Task<Error?> CheckPlacementAsync(
        ProductWriteModel model,
        ICategoryRepository categories,
        ISubCategoryRepository subCategories,
        CancellationToken cancellationToken)
    {
        if (model.CategoryId is { } categoryId
            && !await categories.ExistsAsync(categoryId, cancellationToken))
        {
            return CatalogErrors.CategoryNotFound;
        }

        if (model.SubCategoryId is not { } subCategoryId)
        {
            return null;
        }

        var subCategory = await subCategories.GetByIdAsync(subCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return CatalogErrors.SubCategoryNotFound;
        }

        return model.CategoryId is not null && subCategory.CategoryId != model.CategoryId
            ? CatalogErrors.SubCategoryNotInCategory
            : null;
    }
}