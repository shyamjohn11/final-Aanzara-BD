using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.AddProductImage;

/// <summary>Uploads one file as a product image. The first image on a product
/// becomes primary automatically, mirroring the category-image rule.</summary>
public sealed record AddProductImageCommand : ICommand<Result<ProductImageResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid ProductId { get; init; }

    public FileUpload? File { get; init; }

    public bool IsPrimary { get; init; }
}
