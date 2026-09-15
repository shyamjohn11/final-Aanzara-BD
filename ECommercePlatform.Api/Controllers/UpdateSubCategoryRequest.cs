using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Multipart form shape for sub-category updates.</summary>
public sealed class UpdateSubCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string SubCategoryCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SubCategoryName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;

    public IFormFile? Image { get; init; }
}