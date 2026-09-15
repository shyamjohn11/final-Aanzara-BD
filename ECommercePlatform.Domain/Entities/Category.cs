using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities;

/// <summary>Top level of the product tree.</summary>
public class Category : AuditableEntity
{
    public Guid CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Whether this category is subdivided. Denormalized from the presence of
    /// child rows, so it is maintained on write rather than trusted on read.
    /// </summary>
    public bool HasSubCategory { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SubCategory> SubCategories { get; set; } = [];

    public ICollection<CategoryImage> Images { get; set; } = [];

    public ICollection<Product> Products { get; set; } = [];
}
