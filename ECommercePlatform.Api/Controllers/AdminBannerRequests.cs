using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Multipart form shapes for banners. Kept in the Api layer because
/// <see cref="IFormFile"/> can't live on the Application-layer command.
/// Field names match what admin/banners sends: title, subtitle, link,
/// position, status, startDate, endDate, Image (file).
/// </summary>
public sealed class CreateBannerForm
{
    [Required]
    [MinLength(3)]
    [MaxLength(80)]
    public string? Title { get; init; }

    [MaxLength(150)]
    public string? Subtitle { get; init; }

    public string? Link { get; init; }

    [Required]
    public string? Position { get; init; }

    [Required]
    public string? Status { get; init; }

    [Required]
    public string? StartDate { get; init; }

    [Required]
    public string? EndDate { get; init; }

    /// <summary>Required on create; omit on update to keep the old image.</summary>
    public IFormFile? Image { get; init; }
}

public sealed class UpdateBannerForm
{
    [MinLength(3)]
    [MaxLength(80)]
    public string? Title { get; init; }

    [MaxLength(150)]
    public string? Subtitle { get; init; }

    public string? Link { get; init; }
    public string? Position { get; init; }
    public string? Status { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }

    /// <summary>New file replaces the stored image; omit to keep it.</summary>
    public IFormFile? Image { get; init; }
}

public sealed record BannerStatusRequest(string? Status);
