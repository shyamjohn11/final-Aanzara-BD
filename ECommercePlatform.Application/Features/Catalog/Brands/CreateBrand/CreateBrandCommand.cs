using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.CreateBrand;

public sealed record CreateBrandCommand : ICommand<Result<BrandResponse>>
{
    [Required]
    [MaxLength(150)]
    public string BrandName { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; init; }

    public bool IsOnSale { get; init; }

    public BrandStatus Status { get; init; } = BrandStatus.Active;

    /// <summary>Optional primary image, saved alongside the new brand.</summary>
    public FileUpload? Image { get; init; }
}