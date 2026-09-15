using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Categories.GetProductTree;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// The product tree: the whole Category -> SubCategory hierarchy with product
/// counts, in one call. Intended for navigation menus and category pickers, which
/// would otherwise need a request per level.
/// </summary>
[Authorize]
[Route("api/v1/product-tree")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class ProductTreeController : ApiControllerBase
{
    private readonly ILogger<ProductTreeController> _logger;

    public ProductTreeController(ISender sender, ILogger<ProductTreeController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>
    /// Returns the tree. Pass activeOnly=false to include inactive nodes, which is
    /// what an admin screen wants; the default is the storefront view.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProductTreeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductTreeResponse>> Get(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get action started.");

        try
        {
            var result = await Sender.Send(new GetProductTreeQuery(activeOnly), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Get action finished.");
        }
    }
}
