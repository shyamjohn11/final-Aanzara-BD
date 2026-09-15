using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Dashboard;
using ECommercePlatform.Application.Features.Admin.Orders;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController : ApiControllerBase
{
    public AdminDashboardController(ISender sender) : base(sender) { }

    [HttpGet("stats")] // #69 stat cards + sales chart
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsResponse>> Stats(CancellationToken ct)
        => ToResponse(await Sender.Send(new GetDashboardStatsQuery(), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/orders")]
public sealed class AdminOrdersController : ApiControllerBase
{
    public AdminOrdersController(ISender sender) : base(sender) { }

    [HttpGet] // #71 paged list + filters (?Page=&PageSize=&status=&search=)
    [ProducesResponseType(typeof(PagedResult<OrderAdminResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderAdminResponse>>> List(
        [FromQuery] GetAdminOrdersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("recent")] // #70 RECENT_ORDERS (?count=5)
    [ProducesResponseType(typeof(IReadOnlyList<OrderAdminResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderAdminResponse>>> Recent(
        [FromQuery] int count = 5, CancellationToken ct = default)
        => ToResponse(await Sender.Send(new GetRecentAdminOrdersQuery(count), ct));

    [HttpGet("{orderId:guid}")] // #72 view modal
    [ProducesResponseType(typeof(OrderAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderAdminResponse>> Get(Guid orderId, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetAdminOrderByIdQuery(orderId), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{orderId:guid}/status")] // #73 Pending->Confirmed->Processing->Shipped->Delivered/Cancelled
    [ProducesResponseType(typeof(OrderAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderAdminResponse>> UpdateStatus(
        Guid orderId, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateAdminOrderStatusCommand(orderId, request.Status), ct));

    [HttpDelete("{orderId:guid}")] // #74
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid orderId, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteAdminOrderCommand(orderId), ct));
}
