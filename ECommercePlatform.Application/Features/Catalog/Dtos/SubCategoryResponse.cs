namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record SubCategoryResponse
{
    public Guid SubCategoryId { get; init; }
    public string SubCategoryCode { get; init; } = string.Empty;
    public string SubCategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CategoryId { get; init; }
    public bool IsActive { get; init; }
    public string? ImageUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
