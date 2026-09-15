using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Multipart form shape for brand updates. Kept in the Api layer because
/// <see cref="IFormFile"/> can't live on the Application-layer command without
/// pulling ASP.NET Core into that project.
/// </summary>
public sealed class UpdateBrandRequest
{
    [Required]
    [MaxLength(150)]
    public string BrandName { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; init; }

    public bool IsOnSale { get; init; }

    public BrandStatus Status { get; init; } = BrandStatus.Active;

    /// <summary>Optional new primary image. Replaces the existing one when sent.</summary>
    public IFormFile? Image { get; init; }
}