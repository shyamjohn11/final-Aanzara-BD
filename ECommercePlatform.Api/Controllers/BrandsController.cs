using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.BrandImages.GetBrandImageFile;
using ECommercePlatform.Application.Features.Catalog.Brands.CreateBrand;
using ECommercePlatform.Application.Features.Catalog.Brands.DeleteBrand;
using ECommercePlatform.Application.Features.Catalog.Brands.GetBrandById;
using ECommercePlatform.Application.Features.Catalog.Brands.GetBrands;
using ECommercePlatform.Application.Features.Catalog.Brands.UpdateBrand;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommercePlatform.Application.Features.Catalog.Brands.SearchBrands;
using ECommercePlatform.Application.Features.Catalog.Brands.SetBrandLogo;
using ECommercePlatform.Application.Features.Catalog.Products.GetProducts;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Brands, each with a single primary image.</summary>
[Authorize]
[Route("api/admin/brands")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class BrandsController : ApiControllerBase
{
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(ISender sender, ILogger<BrandsController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Paged brand list with optional search and status filter.</summary>
    [HttpGet]
    [HasPermission(Permissions.Brand.View)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BrandResponse>>> List(
        [FromQuery] GetBrandsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("List action started.");

        try
        {
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List action finished.");
        }
    }

    /// <summary>One brand with its image.</summary>
    [HttpGet("{brandId:guid}", Name = nameof(GetBrand))]
    [HasPermission(Permissions.Brand.View)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandResponse>> GetBrand(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetBrand action started.");

        try
        {
            var result = await Sender.Send(new GetBrandByIdQuery(brandId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetBrand action finished.");
        }
    }

    /// <summary>
    /// Streams the brand's primary image file, resolved by brand id alone.
    /// Anonymous so logos render on public pages too — stored URLs are not
    /// directly servable.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{brandId:guid}/image/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(Guid brandId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetImageFile action started.");

        try
        {
            var result = await Sender.Send(new GetBrandImageFileQuery(brandId), cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            var file = result.Value;

            if (!System.IO.File.Exists(file.FilePath))
            {
                return ToProblem(CatalogErrors.ImageFileMissing);
            }

            // Fixed URL whose content changes on re-upload, so force revalidation
            // instead of letting the browser heuristically cache a stale logo.
            Response.SetImageRevalidationCacheHeaders();

            // enableRangeProcessing lets browsers seek/partially fetch large images.
            return PhysicalFile(file.FilePath, file.ContentType, file.FileName, enableRangeProcessing: true);
        }
        finally
        {
            _logger.LogInformation("GetImageFile action finished.");
        }
    }

    /// <summary>Creates a brand with an optional primary image. Returns 201 with the created row.</summary>
    [HttpPost]
    [HasPermission(Permissions.Brand.Create)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> Create(
        [FromForm] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");

        try
        {
            var command = new CreateBrandCommand
            {
                BrandName = request.BrandName,
                Description = request.Description,
                IsOnSale = request.IsOnSale,
                Status = request.Status,
                Image = request.Image is null
                    ? null
                    : new FileUpload(
                        request.Image.OpenReadStream(),
                        request.Image.FileName,
                        request.Image.ContentType,
                        request.Image.Length)
            };

            var result = await Sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return CreatedAtAction(
                nameof(GetBrand),
                new { brandId = result.Value.BrandId },
                result.Value);
        }
        finally
        {
            _logger.LogInformation("Create action finished.");
        }
    }

    /// <summary>Updates a brand in place. Optionally replaces its primary image.</summary>
    [HttpPut("{brandId:guid}")]
    [HasPermission(Permissions.Brand.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> Update(
        Guid brandId,
        [FromForm] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update action started.");

        try
        {
            var command = new UpdateBrandCommand
            {
                BrandId = brandId,
                BrandName = request.BrandName,
                Description = request.Description,
                IsOnSale = request.IsOnSale,
                Status = request.Status,
                Image = request.Image is null
                    ? null
                    : new FileUpload(
                        request.Image.OpenReadStream(),
                        request.Image.FileName,
                        request.Image.ContentType,
                        request.Image.Length)
            };

            var result = await Sender.Send(command, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Update action finished.");
        }
    }

    /// <summary>Replaces the brand logo from a single uploaded file (no other fields needed).</summary>
    [HttpPatch("{brandId:guid}/logo")]
    [HasPermission(Permissions.Brand.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandResponse>> SetLogo(
        Guid brandId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetLogo action started.");

        try
        {
            var result = await Sender.Send(
                new SetBrandLogoCommand
                {
                    BrandId = brandId,
                    File = file is null
                        ? null
                        : new FileUpload(
                            file.OpenReadStream(),
                            file.FileName,
                            string.IsNullOrWhiteSpace(file.ContentType)
                                ? "application/octet-stream"
                                : file.ContentType,
                            file.Length)
                },
                cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("SetLogo action finished.");
        }
    }

    /// <summary>
    /// Deletes a brand and its image file. No payload.
    /// Refused while products are still assigned to it.
    /// </summary>
    [HttpDelete("{brandId:guid}")]    [HasPermission(Permissions.Brand.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete action started.");

        try
        {
            var result = await Sender.Send(new DeleteBrandCommand(brandId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete action finished.");
        }
    }

    /// <summary>Unpaged keyword search, capped to a small result set — for typeahead-style lookups.</summary>
[HttpGet("search")]
[HasPermission(Permissions.Brand.View)]
[ProducesResponseType(typeof(IReadOnlyList<BrandResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<IReadOnlyList<BrandResponse>>> Search(
    [FromQuery] string? q,
    [FromQuery] string? query,
    [FromQuery(Name = "keyword")] string? keyword,
    [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
{
    // Frontend debounced search sends ?query=; keep ?q=/?keyword= working too.
    var result = await Sender.Send(new SearchBrandsQuery(query ?? keyword ?? q, limit), cancellationToken);

    return ToResponse(result);
}

/// <summary>Active products belonging to this brand, paged.</summary>
[HttpGet("{brandId:guid}/products")]
[HasPermission(Permissions.Product.View)]
[ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResult<ProductSummaryResponse>>> GetProducts(
    Guid brandId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25,
    CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(
        new GetProductsQuery { BrandId = brandId, Page = page, PageSize = pageSize }, cancellationToken);

    return ToResponse(result);
}
}