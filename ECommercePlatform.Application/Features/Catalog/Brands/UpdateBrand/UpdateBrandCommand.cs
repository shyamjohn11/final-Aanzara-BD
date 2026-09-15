using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.UpdateBrand;

public sealed record UpdateBrandCommand : ICommand<Result<BrandResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid BrandId { get; init; }

    [Required]
    [MaxLength(150)]
    public string BrandName { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; init; }

    public bool IsOnSale { get; init; }

    public BrandStatus Status { get; init; } = BrandStatus.Active;

    /// <summary>Optional new primary image. Replaces the existing one when sent.</summary>
    public FileUpload? Image { get; init; }
}