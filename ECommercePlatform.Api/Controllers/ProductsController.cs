using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Application.Features.Catalog.Products.CreateProduct;
using ECommercePlatform.Application.Features.Catalog.Products.DeleteProduct;
using ECommercePlatform.Application.Features.Catalog.Products.GetProductById;
using ECommercePlatform.Application.Features.Catalog.Products.GetProducts;
using ECommercePlatform.Application.Features.Catalog.Products.UpdateProduct;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;
using ECommercePlatform.Application.Features.Catalog.Products.GetFreshArrivals;
using ECommercePlatform.Application.Features.Catalog.Products.GetProductsByCategory;
using ECommercePlatform.Application.Features.Catalog.Products.GetPopularProducts;
using ECommercePlatform.Application.Features.Catalog.Products.GetLowStockProducts;
using ECommercePlatform.Application.Features.Catalog.Products.GetInStockProducts;
/// <summary>Leaves of the product tree.</summary>
[Authorize]
[Route("api/v1/products")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class ProductsController : ApiControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ISender sender, ILogger<ProductsController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>
    /// Paged product search. Filter by categoryId or subCategoryId to walk down
    /// from the product tree into the leaves.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductSummaryResponse>>> List(
        [FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
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

    /// <summary>One product, in full.</summary>
    [HttpGet("{productId:guid}", Name = nameof(GetProduct))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        Guid productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProduct action started.");

        try
        {
            var result = await Sender.Send(new GetProductByIdQuery(productId), cancellationToken);
                
            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetProduct action finished.");
        }
    }

    /// <summary>Creates a product. Returns 201 with the created row.</summary>
    [HttpPost]
    [HasPermission(Permissions.Product.Create)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");

        try
        {
            var result = await Sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return CreatedAtAction(
                nameof(GetProduct),
                new { productId = result.Value.ProductId },
                result.Value);
        }
        finally
        {
            _logger.LogInformation("Create action finished.");
        }
    }

    /// <summary>Updates a product in place.</summary>
    [HttpPut("{productId:guid}")]
    [HasPermission(Permissions.Product.Update)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid productId,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update action started.");

        try
        {
            var result = await Sender.Send(
                command with { ProductId = productId }, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Update action finished.");
        }
    }

    /// <summary>Deletes a product. No payload.</summary>
    [HttpDelete("{productId:guid}")]
    [HasPermission(Permissions.Product.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete action started.");

        try
        {
            var result = await Sender.Send(new DeleteProductCommand(productId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete action finished.");
        }
    }

    /// <summary>Most recently added active products, newest first.</summary>
[HttpGet("fresh-arrivals")]
[AllowAnonymous]
[ProducesResponseType(typeof(IReadOnlyList<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<IReadOnlyList<ProductSummaryResponse>>> GetFreshArrivals(
    [FromQuery] int count = 10, CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(new GetFreshArrivalsQuery(count), cancellationToken);

    return ToResponse(result);
}

/// <summary>Active products with the deepest discounts first — the Popular shelf.</summary>
[HttpGet("popular")]
[AllowAnonymous]
[ProducesResponseType(typeof(IReadOnlyList<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<IReadOnlyList<ProductSummaryResponse>>> GetPopular(
    [FromQuery] int count = 8, CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(new GetPopularProductsQuery(count), cancellationToken);

    return ToResponse(result);
}

/// <summary>Active products at or below reorder level, scarcest first.</summary>
[HttpGet("low-stock")]
[AllowAnonymous]
[ProducesResponseType(typeof(IReadOnlyList<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<IReadOnlyList<ProductSummaryResponse>>> GetLowStock(
    [FromQuery] int count = 8, CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(new GetLowStockProductsQuery(count), cancellationToken);

    return ToResponse(result);
}

/// <summary>Active products that are tracked and above reorder level, or untracked.</summary>
[HttpGet("in-stock")]
[AllowAnonymous]
[ProducesResponseType(typeof(IReadOnlyList<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<IReadOnlyList<ProductSummaryResponse>>> GetInStock(
    [FromQuery] int count = 8, CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(new GetInStockProductsQuery(count), cancellationToken);

    return ToResponse(result);
}

/// <summary>Active products within a single category, paged.</summary>
[HttpGet("category/{categoryId:guid}")]
[AllowAnonymous]
[ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResult<ProductSummaryResponse>>> GetProductsByCategory(
    Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
{
    var result = await Sender.Send(
        new GetProductsByCategoryQuery(categoryId, page, pageSize), cancellationToken);

    return ToResponse(result);
}
}