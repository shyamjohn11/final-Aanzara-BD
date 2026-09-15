using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages;

/// <summary>
/// Product-image mapping, kept here (not in CatalogMappings) so no existing
/// file has to change to support the new product-image endpoints.
/// </summary>
internal static class ProductImageMappings
{
    internal static ProductImageResponse ToResponse(this ProductImage i) => new()
    {
        ImageId = i.ImageId,
        ProductId = i.ProductId,
        // Stored URLs point at a host nothing serves; hand back the anonymous
        // streaming route instead (same pattern as category/brand images).
        ImageUrl = $"/api/v1/products/{i.ProductId}/images/{i.ImageId}/file",
        IsPrimary = i.IsPrimary,
        DisplayOrder = i.DisplayOrder,
        CreatedAt = i.CreatedAt
    };
}
