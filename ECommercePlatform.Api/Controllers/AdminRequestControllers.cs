using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Enquiries;
using ECommercePlatform.Application.Features.Admin.PricingRequests;
using ECommercePlatform.Application.Features.Admin.Quotes;
using ECommercePlatform.Application.Features.Admin.Reviews;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/quotes")]
public sealed class AdminQuotesController : ApiControllerBase
{
    public AdminQuotesController(ISender sender) : base(sender) { }

    [HttpGet] // #122
    [ProducesResponseType(typeof(PagedResult<QuoteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<QuoteResponse>>> List(
        [FromQuery] GetQuotesQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminQuote")] // #123
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetQuoteByIdQuery(id), ct));

    [HttpPost] // #124
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<QuoteResponse>> Create(
        [FromBody] CreateQuoteCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminQuote", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #125
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteResponse>> Update(
        Guid id, [FromBody] UpdateQuoteCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #126
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteQuoteCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateQuoteStatusCommand(id, request.Status), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/reviews")]
public sealed class AdminReviewsController : ApiControllerBase
{
    public AdminReviewsController(ISender sender) : base(sender) { }

    [HttpGet] // #127 list
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> List(
        [FromQuery] GetReviewsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}")] // #128 details
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetReviewByIdQuery(id), ct));

    [HttpPost] // A1 admin add (re-enables the Add button)
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReviewResponse>> Create(
        [FromBody] CreateReviewCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // A1 admin edit (re-enables the Edit button)
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewResponse>> Update(
        Guid id, [FromBody] UpdateReviewCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #129 approve/hide
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateReviewStatusCommand(id, request.Status), ct));

    [HttpDelete("{id:guid}")] // #130
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteReviewCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/enquiries")]
public sealed class AdminEnquiriesController : ApiControllerBase
{
    public AdminEnquiriesController(ISender sender) : base(sender) { }

    [HttpGet] // #131 list
    [ProducesResponseType(typeof(PagedResult<EnquiryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EnquiryResponse>>> List(
        [FromQuery] GetEnquiriesQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}")] // #132 details
    [ProducesResponseType(typeof(EnquiryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnquiryResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetEnquiryByIdQuery(id), ct));

    [HttpPost] // A2 admin add (re-enables the Add button)
    [ProducesResponseType(typeof(EnquiryResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EnquiryResponse>> Create(
        [FromBody] CreateEnquiryCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // A2 admin edit (re-enables the Edit button)
    [ProducesResponseType(typeof(EnquiryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnquiryResponse>> Update(
        Guid id, [FromBody] UpdateEnquiryCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #133 resolve
    [ProducesResponseType(typeof(EnquiryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnquiryResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateEnquiryStatusCommand(id, request.Status), ct));

    [HttpDelete("{id:guid}")] // #134
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteEnquiryCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/pricing-requests")]
public sealed class AdminPricingRequestsController : ApiControllerBase
{
    public AdminPricingRequestsController(ISender sender) : base(sender) { }

    [HttpGet] // #135 list
    [ProducesResponseType(typeof(PagedResult<PricingRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PricingRequestResponse>>> List(
        [FromQuery] GetPricingRequestsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}")] // #136 details
    [ProducesResponseType(typeof(PricingRequestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PricingRequestResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetPricingRequestByIdQuery(id), ct));

    [HttpPost] // A3 admin add (re-enables the Add button)
    [ProducesResponseType(typeof(PricingRequestResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PricingRequestResponse>> Create(
        [FromBody] CreatePricingRequestCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // A3 admin edit (re-enables the Edit button)
    [ProducesResponseType(typeof(PricingRequestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PricingRequestResponse>> Update(
        Guid id, [FromBody] UpdatePricingRequestCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #137 approve/reject
    [ProducesResponseType(typeof(PricingRequestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PricingRequestResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdatePricingRequestStatusCommand(id, request.Status), ct));

    [HttpDelete("{id:guid}")] // #138
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeletePricingRequestCommand(id), ct));
}
