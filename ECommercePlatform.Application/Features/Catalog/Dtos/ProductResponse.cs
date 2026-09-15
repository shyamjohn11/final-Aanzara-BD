namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record ProductResponse
{
    public Guid ProductId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public decimal Mrp { get; init; }
    public decimal Discount { get; init; }
    public int Moq { get; init; }
    public bool IsOrganic { get; init; }
    public bool IsGstFree { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Trimmed shape for list endpoints — enough to render a row, no more.</summary>
public sealed record ProductSummaryResponse
{
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public decimal Price { get; init; }
    public decimal Mrp { get; init; }
    public decimal Discount { get; init; }
    public int Moq { get; init; }
    public string Status { get; init; } = string.Empty;

    // Display-only fields filled in by ProductSummaryEnrichment; null means the
    // product has no such data (no brand, no image, no inventory rows).
    public string? BrandName { get; init; }
    public string? ImageUrl { get; init; }
    public int? AvailableStock { get; init; }
    public string? StockStatus { get; init; }
}