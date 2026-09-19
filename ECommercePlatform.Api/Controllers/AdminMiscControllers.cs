using System.Text.Json;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Admin.Reports;
using ECommercePlatform.Application.Features.Admin.Settings;
using ECommercePlatform.Application.Features.Admin.WholesalePricing;
using ECommercePlatform.Application.Features.Admin.WishlistInsights;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/notifications")]
public sealed class AdminNotificationsController : ApiControllerBase
{
    public AdminNotificationsController(ISender sender) : base(sender) { }

    [HttpGet] // #139 list
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> List(
        [FromQuery] GetNotificationsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    public sealed record MarkReadRequest(bool IsRead = true);

    [HttpPatch("{id:guid}/read")] // #140 mark read
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        Guid id, [FromBody] MarkReadRequest? request, CancellationToken ct)
        => ToResponse(await Sender.Send(
            new MarkNotificationReadCommand(id, request?.IsRead ?? true), ct));

    [HttpDelete("{id:guid}")] // #141 dismiss
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteNotificationCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/reports")]
public sealed class AdminReportsController : ApiControllerBase
{
    public AdminReportsController(ISender sender) : base(sender) { }

    [HttpGet] // #142 one call: summary + chart series + top products
    [ProducesResponseType(typeof(ReportsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportsResponse>> Get(
        [FromQuery] GetReportsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/settings")]
public sealed class AdminSettingsController : ApiControllerBase
{
    public AdminSettingsController(ISender sender) : base(sender) { }

    [HttpGet] // #143 load
    [ProducesResponseType(typeof(Dictionary<string, JsonElement>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, JsonElement>>> Load(CancellationToken ct)
        => ToResponse(await Sender.Send(new GetSettingsQuery(), ct));

    [HttpPut] // #144 save
    [ProducesResponseType(typeof(Dictionary<string, JsonElement>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, JsonElement>>> Save(
        [FromBody] Dictionary<string, JsonElement> values, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateSettingsCommand(values ?? new()), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/wholesale-pricing")]
public sealed class AdminWholesalePricingController : ApiControllerBase
{
    public AdminWholesalePricingController(ISender sender) : base(sender) { }

    [HttpGet] // #145 price list
    [HasPermission(Permissions.Wholesale.View)]
    [ProducesResponseType(typeof(PagedResult<WholesalePriceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WholesalePriceResponse>>> List(
        [FromQuery] GetWholesalePricesQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpPut] // #146 bulk price update
    [HasPermission(Permissions.Wholesale.Update)]
    [ProducesResponseType(typeof(IReadOnlyList<WholesalePriceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WholesalePriceResponse>>> BulkUpdate(
        [FromBody] List<WholesalePriceUpdateDto> items, CancellationToken ct)
        => ToResponse(await Sender.Send(new BulkUpdateWholesalePricesCommand(items ?? new List<WholesalePriceUpdateDto>()), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/wishlist-insights")]
public sealed class AdminWishlistInsightsController : ApiControllerBase
{
    public AdminWishlistInsightsController(ISender sender) : base(sender) { }

    [HttpGet] // #147 most-wishlisted analytics, read-only
    [ProducesResponseType(typeof(PagedResult<WishlistInsightResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WishlistInsightResponse>>> List(
        [FromQuery] GetWishlistInsightsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));
}
