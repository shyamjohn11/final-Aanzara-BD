namespace ECommercePlatform.Application.Features.Catalog.Dtos;

/// <summary>
/// The product tree: categories, their sub-categories, and how many products hang
/// off each. Shaped for a navigation sidebar, so it carries counts rather than the
/// products themselves.
/// </summary>
public sealed record ProductTreeResponse
{
    public IReadOnlyCollection<CategoryTreeResponse> Categories { get; init; } = [];
    public int TotalCategories { get; init; }
    public int TotalSubCategories { get; init; }
    public int TotalProducts { get; init; }
}

public sealed record CategoryTreeResponse
{
    public Guid CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public int ProductCount { get; init; }
    public IReadOnlyCollection<SubCategoryTreeResponse> SubCategories { get; init; } = [];
}

public sealed record SubCategoryTreeResponse
{
    public Guid SubCategoryId { get; init; }
    public string SubCategoryCode { get; init; } = string.Empty;
    public string SubCategoryName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public int ProductCount { get; init; }
}
