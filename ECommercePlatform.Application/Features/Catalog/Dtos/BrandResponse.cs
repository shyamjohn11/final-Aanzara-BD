using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record BrandResponse
{
    public Guid BrandId { get; init; }
    public string BrandName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsOnSale { get; init; }
    public BrandStatus Status { get; init; }

    /// <summary>The primary image, or the only image if none is marked primary; null if the brand has none.</summary>
    public string? ImageUrl { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}