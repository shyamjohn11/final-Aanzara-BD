using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog;

public static class CatalogErrors
{
    // -- Categories --
    public static readonly Error CategoryNotFound =
        Error.NotFound("catalog.category_not_found", "The category could not be found.");

    public static Error CategoryCodeTaken(string code) => Error.Conflict(
        "catalog.category_code_taken", $"Category code '{code}' is already in use.");

    public static readonly Error CategoryHasChildren = Error.Conflict(
        "catalog.category_has_children",
        "This category still has sub-categories or products. Remove or reassign them first.");

    // -- Sub-categories --
    public static readonly Error SubCategoryNotFound =
        Error.NotFound("catalog.subcategory_not_found", "The sub-category could not be found.");

    public static Error SubCategoryCodeTaken(string code) => Error.Conflict(
        "catalog.subcategory_code_taken", $"Sub-category code '{code}' is already in use.");

    public static readonly Error SubCategoryHasProducts = Error.Conflict(
        "catalog.subcategory_has_products",
        "This sub-category still has products. Remove or reassign them first.");

    /// <summary>
    /// A product's sub-category must hang off its stated category, or the tree
    /// would be inconsistent.
    /// </summary>
    public static readonly Error SubCategoryNotInCategory = Error.Validation(
        "catalog.subcategory_category_mismatch",
        "The sub-category does not belong to the specified category.");

    // -- Category images --
    public static readonly Error ImageNotFound =
        Error.NotFound("catalog.category_image_not_found", "The category image could not be found.");

    public static readonly Error ImageFileMissing = Error.NotFound(
    "catalog.category_image_file_missing",
    "The image record exists but its file is no longer available.");

    public static readonly Error RequiredImage = Error.Validation(
    "catalog.category_required_image",
    "An image file is required for the category.");

    public static readonly Error CategoryHasNoImages = Error.NotFound(
    "catalog.category_has_no_images", "This category has no images.");

    public static readonly Error SubCategoryHasNoImages = Error.NotFound(
    "catalog.subcategory_has_no_images", "This sub-category has no images.");

    // -- Products --
    public static readonly Error ProductNotFound =
        Error.NotFound("catalog.product_not_found", "The product could not be found.");

    public static readonly Error InvalidFreshArrivalsCount = Error.Validation(
    "catalog.invalid_fresh_arrivals_count", "Count must be greater than zero.");

    public static readonly Error InvalidProductCount = Error.Validation(
    "catalog.invalid_product_count", "Count must be greater than zero.");

    public static Error SkuTaken(string sku) => Error.Conflict(
        "catalog.sku_taken", $"SKU '{sku}' is already in use.");

    public static Error InvalidStatus(string? status) => Error.Validation(
        "catalog.invalid_product_status",
        $"'{status}' is not a valid product status. Use Active or Inactive.");

    // -- Brands --
    public static readonly Error BrandNotFound =
        Error.NotFound("catalog.brand_not_found", "The brand could not be found.");

    public static Error BrandNameTaken(string name) => Error.Conflict(
        "catalog.brand_name_taken", $"Brand name '{name}' is already in use.");

    public static readonly Error BrandHasProducts = Error.Conflict(
        "catalog.brand_has_products",
        "This brand still has products. Remove or reassign them first.");
}