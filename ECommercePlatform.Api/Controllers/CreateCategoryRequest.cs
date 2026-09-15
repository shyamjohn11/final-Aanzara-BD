using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Multipart form shape for category creation. Kept in the Api layer because
/// <see cref="IFormFile"/> can't live on the Application-layer command without
/// pulling ASP.NET Core into that project.
/// </summary>
public sealed class CreateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string CategoryCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CategoryName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool HasSubCategory { get; init; }

    public bool IsActive { get; init; } = true;

    [Required(ErrorMessage = "An image file is required.")]
    public IFormFile? Image { get; init; }
}