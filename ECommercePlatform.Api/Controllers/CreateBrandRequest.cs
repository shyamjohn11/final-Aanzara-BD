using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Multipart form shape for brand creation. Kept in the Api layer because
/// <see cref="IFormFile"/> can't live on the Application-layer command without
/// pulling ASP.NET Core into that project.
/// </summary>
public sealed class CreateBrandRequest
{
    [Required]
    [MaxLength(150)]
    public string BrandName { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; init; }

    public bool IsOnSale { get; init; }

    public BrandStatus Status { get; init; } = BrandStatus.Active;

    public IFormFile? Image { get; init; }
}