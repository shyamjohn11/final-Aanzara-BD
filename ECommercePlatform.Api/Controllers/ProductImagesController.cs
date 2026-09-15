using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Application.Features.Catalog.ProductImages.AddProductImage;
using ECommercePlatform.Application.Features.Catalog.ProductImages.DeleteProductImage;
using ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImageFile;
using ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImages;
using ECommercePlatform.Application.Features.Catalog.ProductImages.SetPrimaryProductImage;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Product images. The admin products page picker previews a file but the
/// create/update payloads carry no image field, so uploads live here as
/// multipart actions — same shape as the category-image endpoints.
/// </summary>
[Authorize]
[Route("api/v1/products/{productId:guid}/images")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class ProductImagesController : ApiControllerBase
{
    private readonly ILogger<ProductImagesController> _logger;

    public ProductImagesController(ISender sender, ILogger<ProductImagesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Lists a product's images, in display order.</summary>
    [HttpGet]
    [HasPermission(Permissions.Product.View)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductImageResponse>>> List(
        Guid productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("List product images action started.");

        try
        {
            var result = await Sender.Send(new GetProductImagesQuery(productId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List product images action finished.");
        }
    }

    /// <summary>Streams one image file. Anonymous — img tags hit this directly.</summary>
    [AllowAnonymous]
    [HttpGet("{imageId:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetImageFile action started.");

        try
        {
            var result = await Sender.Send(
                new GetProductImageFileQuery(productId, imageId), cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            var file = result.Value;

            if (!System.IO.File.Exists(file.FilePath))
            {
                return ToProblem(CatalogErrors.ImageFileMissing);
            }

            Response.SetImageRevalidationCacheHeaders();

            return PhysicalFile(file.FilePath, file.ContentType, file.FileName, enableRangeProcessing: true);
        }
        finally
        {
            _logger.LogInformation("GetImageFile action finished.");
        }
    }

    /// <summary>Uploads one image file for a product. First image becomes primary.</summary>
    [HttpPost]
    [HttpPost("/api/v1/products/{productId:guid}/image")]
    [HasPermission(Permissions.Product.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageResponse>> Upload(
        Guid productId,
        IFormFile file,
        [FromForm] bool isPrimary = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Upload product image action started.");

        try
        {
            if (file is null || file.Length <= 0)
            {
                return ToProblem(Error.Validation(
                    "catalog.product_image_required", "An image file is required."));
            }

            var result = await Sender.Send(
                new AddProductImageCommand
                {
                    ProductId = productId,
                    File = new FileUpload(
                        file.OpenReadStream(),
                        file.FileName,
                        string.IsNullOrWhiteSpace(file.ContentType)
                            ? "application/octet-stream"
                            : file.ContentType,
                        file.Length),
                    IsPrimary = isPrimary
                },
                cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return CreatedAtAction(
                nameof(List),
                new { productId },
                result.Value);
        }
        finally
        {
            _logger.LogInformation("Upload product image action finished.");
        }
    }

    /// <summary>Promotes an image to primary, demoting the incumbent.</summary>
    [HttpPost("{imageId:guid}/primary")]
    [HasPermission(Permissions.Product.Update)]
    [ProducesResponseType(typeof(ProductImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageResponse>> SetPrimary(
        Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new SetPrimaryProductImageCommand(productId, imageId), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>Deletes an image and its file. Promotes the next image if needed.</summary>
    [HttpDelete("{imageId:guid}")]
    [HasPermission(Permissions.Product.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(
        Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new DeleteProductImageCommand(productId, imageId), cancellationToken);

        return ToNoContent(result);
    }
}
