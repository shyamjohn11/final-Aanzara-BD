using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.GetNearestWarehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/warehouses")]
public sealed class StorefrontWarehousesController : ApiControllerBase
{
    public StorefrontWarehousesController(ISender sender) : base(sender) { }

    /// <summary>Find nearest warehouse with stock for a product and customer location.</summary>
    [HttpGet("nearest")]
    [ProducesResponseType(typeof(GetNearestWarehouseResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetNearestWarehouseResponse>> Nearest(
        [FromQuery] Guid productId,
        [FromQuery] string city,
        [FromQuery] string state,
        [FromQuery] string? pincode,
        [FromQuery] double? lat,
        [FromQuery] double? lon,
        CancellationToken ct)
        => ToResponse(await Sender.Send(new GetNearestWarehouseQuery(productId, city, state, pincode, lat, lon), ct));
}
