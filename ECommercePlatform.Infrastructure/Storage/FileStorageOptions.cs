using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Infrastructure.Storage;

/// <summary>
/// Bound from the "FileStorage" section and validated at startup, so a missing
/// or invalid folder fails the host immediately rather than on the first upload.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Physical root uploads are written under, e.g. D:\ECommerceStorage.</summary>
    [Required(AllowEmptyStrings = false)]
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Base URL files are served from, e.g. https://localhost:5001/uploads.</summary>
    [Required(AllowEmptyStrings = false)]
    public string PublicBaseUrl { get; set; } = string.Empty;

    [Range(1, 50)]
    public int MaxFileSizeMb { get; set; } = 5;
}