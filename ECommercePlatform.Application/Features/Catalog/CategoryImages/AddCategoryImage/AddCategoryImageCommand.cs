using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.CategoryImages.AddCategoryImage;

/// <summary>
/// Registers image metadata against a category. The bytes are expected to be
/// uploaded to storage separately; this records where they landed.
/// </summary>
public sealed record AddCategoryImageCommand : ICommand<Result<CategoryImageResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid CategoryId { get; init; }

    [Required]
    [MaxLength(2000)]
    [Url]
    public string ImageUrl { get; init; } = string.Empty;

    [Required]
    [MaxLength(260)]
    public string FileName { get; init; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; init; } = string.Empty;

    [Range(0, long.MaxValue)]
    public long FileSize { get; init; }

    public bool IsPrimary { get; init; }
}
