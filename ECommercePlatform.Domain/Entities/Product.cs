using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities;

/// <summary>Leaf of the product tree, optionally hanging off a category, sub-category and brand.</summary>
public class Product : AuditableEntity
{
    public Guid ProductId { get; set; }

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }

    public Guid? SubCategoryId { get; set; }

    public SubCategory? SubCategory { get; set; }

    public Guid? BrandId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Money is decimal, never double: binary floating point cannot represent
    // most currency values exactly.
    public decimal Price { get; set; }

    public decimal Mrp { get; set; }

    public decimal Discount { get; set; }

    /// <summary>Minimum order quantity.</summary>
    public int Moq { get; set; } = 1;

    public bool IsOrganic { get; set; }

    public bool IsGstFree { get; set; }

    public string Status { get; set; } = ProductStatus.Active;
}

/// <summary>
/// The schema stores Status as free text, so these constants keep writers honest
/// without a migration to change the column type.
/// </summary>
public static class ProductStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Active, Inactive };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}