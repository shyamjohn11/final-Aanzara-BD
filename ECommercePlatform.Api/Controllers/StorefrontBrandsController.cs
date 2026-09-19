using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Application.Features.Shop.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Public storefront brand shelf (active brands only). No authentication
/// required, mirroring the product shelf and deals endpoints. Brand
/// management stays on the permission-gated admin brands controller.
/// </summary>
[AllowAnonymous]
[Route("api/v1/brands")]
public sealed class StorefrontBrandsController : ApiControllerBase
{
    private readonly ILogger<StorefrontBrandsController> _logger;

    public StorefrontBrandsController(ISender sender, ILogger<StorefrontBrandsController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Active brands for storefront shelves, optionally name-filtered.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrandResponse>>> List(
        [FromQuery] string? search, [FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("List action started.");

        try
        {
            var result = await Sender.Send(new GetActiveBrandsQuery(search, count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List action finished.");
        }
    }
}
