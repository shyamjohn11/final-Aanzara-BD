namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record SubCategoryImageResponse
{
    public Guid ImageId { get; init; }
    public Guid SubCategoryId { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}