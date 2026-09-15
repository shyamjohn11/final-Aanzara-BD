using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Content;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

// Site content (testimonials, FAQs, trust badges, hero copy, ...) served
// to the storefront and managed by admin. Additive only: no existing
// controller is modified.

[Authorize]
[Route("api/v1/content")]
public sealed class ContentController : ApiControllerBase
{
    public ContentController(ISender sender) : base(sender) { }

    [HttpGet] // #149 ?section=&search= — active items, sort order
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ContentItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContentItemResponse>>> List(
        [FromQuery] GetPublicContentQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));
}

[Route("api/v1/newsletter")]
public sealed class NewsletterController : ApiControllerBase
{
    public NewsletterController(ISender sender) : base(sender) { }

    [HttpPost("subscribe")] // #150 {email, source} — idempotent
    [AllowAnonymous]
    [ProducesResponseType(typeof(NewsletterSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NewsletterSubscriptionResponse>> Subscribe(
        [FromBody] SubscribeNewsletterCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command, ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/content")]
public sealed class AdminContentController : ApiControllerBase
{
    public AdminContentController(ISender sender) : base(sender) { }

    [HttpGet] // #151 ?Section=&Search=&IsActive=&Page=&PageSize=
    [ProducesResponseType(typeof(PagedResult<ContentItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ContentItemResponse>>> List(
        [FromQuery] GetContentAdminQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminContent")] // #152
    [ProducesResponseType(typeof(ContentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentItemResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetContentByIdQuery(id), ct));

    [HttpPost] // #153
    [ProducesResponseType(typeof(ContentItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContentItemResponse>> Create(
        [FromBody] CreateContentCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminContent", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #154
    [ProducesResponseType(typeof(ContentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentItemResponse>> Update(
        Guid id, [FromBody] UpdateContentCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    public sealed record ContentStatusRequest(bool IsActive);

    [HttpPatch("{id:guid}/status")] // #155 {isActive}
    [ProducesResponseType(typeof(ContentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentItemResponse>> SetStatus(
        Guid id, [FromBody] ContentStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new SetContentStatusCommand(id, request.IsActive), ct));

    [HttpDelete("{id:guid}")] // #156 — 204
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteContentCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/newsletter")]
public sealed class AdminNewsletterController : ApiControllerBase
{
    public AdminNewsletterController(ISender sender) : base(sender) { }

    [HttpGet] // #157 ?Search=&Page=&PageSize=
    [ProducesResponseType(typeof(PagedResult<NewsletterSubscriptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NewsletterSubscriptionResponse>>> List(
        [FromQuery] GetNewsletterSubscriptionsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpDelete("{id:guid}")] // #158 — 204
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteNewsletterSubscriptionCommand(id), ct));
}
