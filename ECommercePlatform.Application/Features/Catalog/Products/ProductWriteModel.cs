using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Application.Features.Catalog.Products;

/// <summary>
/// Fields shared by product create and update. Kept in one base record so the two
/// commands cannot drift apart in validation rules.
/// </summary>
public abstract record ProductWriteModel
{
    public Guid? CategoryId { get; init; }

    public Guid? SubCategoryId { get; init; }

    public Guid? BrandId { get; init; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Sku { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [MaxLength(5000)]
    public string? Specification { get; init; }

    [Range(0, 99999999.99)]
    public decimal Price { get; init; }

    [Range(0, 99999999.99)]
    public decimal Mrp { get; init; }

    [Range(0, 100)]
    public decimal Discount { get; init; }

    [Range(1, int.MaxValue)]
    public int Moq { get; init; } = 1;

    public bool IsOrganic { get; init; }

    public bool IsGstFree { get; init; }

    [Required]
    [MaxLength(20)]
    public string Status { get; init; } = Domain.Entities.ProductStatus.Active;
}