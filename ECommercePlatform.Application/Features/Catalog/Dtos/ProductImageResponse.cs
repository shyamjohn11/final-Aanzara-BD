namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record ProductImageResponse
{
    public Guid ImageId { get; init; }
    public Guid ProductId { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
