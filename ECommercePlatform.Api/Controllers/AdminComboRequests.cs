using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Multipart form shapes for combos. Kept in the Api layer because
/// <see cref="IFormFile"/> can't live on the Application-layer command.
/// Field names: name, title, description, productIds (repeatable),
/// price, originalPrice, status, Image (file).
/// </summary>
public sealed class CreateComboForm
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    public List<Guid>? ProductIds { get; init; }
    [Range(0, 10000000)] public decimal Price { get; init; }
    [Range(0, 10000000)] public decimal OriginalPrice { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    /// <summary>Optional on create; without it the URL string (if any) is kept.</summary>
    public IFormFile? Image { get; init; }
}

public sealed class UpdateComboForm
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(4000)] public string? Description { get; init; }
    public List<Guid>? ProductIds { get; init; }
    public decimal? Price { get; init; }
    public decimal? OriginalPrice { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    /// <summary>New file replaces the stored image; omit to keep it.</summary>
    public IFormFile? Image { get; init; }
}

/// <summary>Shared IFormFile → FileUpload mapper for the admin form endpoints.</summary>
internal static class AdminFormFiles
{
    internal static FileUpload? ToFileUpload(IFormFile? file) => file is null
        ? null
        : new FileUpload(
            file.OpenReadStream(),
            file.FileName,
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            file.Length);
}
