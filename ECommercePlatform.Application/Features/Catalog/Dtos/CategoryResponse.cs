namespace ECommercePlatform.Application.Features.Catalog.Dtos;

public sealed record CategoryResponse
{
    public Guid CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool HasSubCategory { get; init; }
    public bool IsActive { get; init; }

    /// <summary>
    /// Servable streaming URL for the category's primary image; null when the
    /// category has no images, so clients can skip the /image/file request
    /// instead of eating a 404.
    /// </summary>
    public string? PrimaryImageUrl { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record CategoryDetailResponse
{
    public Guid CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool HasSubCategory { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<CategoryImageResponse> Images { get; init; } = [];
}

public sealed record CategoryImageResponse
{
    public Guid CategoryImageId { get; init; }
    public Guid CategoryId { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Where a category image's bytes live on disk, plus enough metadata to stream
/// them back with the right content type and download name. Never serialized
/// as JSON — this backs a raw file response, not an API payload.
/// </summary>
public sealed record CategoryImageFileResponse(string FilePath, string ContentType, string FileName);