namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record CategoryLookupResponse
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
