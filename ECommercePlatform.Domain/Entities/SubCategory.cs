using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities;

/// <summary>Middle level of the product tree.</summary>
public class SubCategory : AuditableEntity
{
    public Guid SubCategoryId { get; set; }

    public string SubCategoryCode { get; set; } = string.Empty;

    public string SubCategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
}
