using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Api.Controllers;

public sealed class CreateProductRequest
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

    public string? Description { get; init; }

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

    public string Status { get; init; } = ProductStatus.Active;

    /// <summary>Any number of images. The first becomes primary.</summary>
    public List<IFormFile>? Images { get; init; }
}