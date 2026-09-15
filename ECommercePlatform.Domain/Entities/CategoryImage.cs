using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities;

/// <summary>
/// Image metadata for a category. Only the location and descriptive fields live
/// here; the bytes themselves belong in blob storage or on disk at FilePath.
/// </summary>
public class CategoryImage : AuditableEntity
{
    public Guid CategoryImageId { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    /// <summary>At most one image per category may carry this; enforced by a filtered unique index.</summary>
    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}
