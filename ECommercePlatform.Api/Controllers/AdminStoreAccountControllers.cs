using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.AgentOnboarding;
using ECommercePlatform.Application.Features.Admin.BusinessAccounts;
using ECommercePlatform.Application.Features.Admin.StoreOffers;
using ECommercePlatform.Application.Features.Admin.Stores;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/stores")]
public sealed class AdminStoresController : ApiControllerBase
{
    public AdminStoresController(ISender sender) : base(sender) { }

    [HttpGet] // #100
    [ProducesResponseType(typeof(PagedResult<StoreAdminResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StoreAdminResponse>>> List(
        [FromQuery] GetStoresQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminStore")] // #101
    [ProducesResponseType(typeof(StoreAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreAdminResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetStoreByIdQuery(id), ct));

    [HttpPost] // #102
    [ProducesResponseType(typeof(StoreAdminResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StoreAdminResponse>> Create(
        [FromBody] CreateStoreCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminStore", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #103
    [ProducesResponseType(typeof(StoreAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreAdminResponse>> Update(
        Guid id, [FromBody] UpdateStoreCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #104
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteStoreCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // activate/deactivate
    [ProducesResponseType(typeof(StoreAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreAdminResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateStoreStatusCommand(id, request.Status), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/store-offers")]
public sealed class AdminStoreOffersController : ApiControllerBase
{
    public AdminStoreOffersController(ISender sender) : base(sender) { }

    [HttpGet] // #105
    [ProducesResponseType(typeof(PagedResult<StoreOfferResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StoreOfferResponse>>> List(
        [FromQuery] GetStoreOffersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminStoreOffer")] // #106
    [ProducesResponseType(typeof(StoreOfferResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreOfferResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetStoreOfferByIdQuery(id), ct));

    [HttpPost] // #107
    [ProducesResponseType(typeof(StoreOfferResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StoreOfferResponse>> Create(
        [FromBody] CreateStoreOfferCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminStoreOffer", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #108
    [ProducesResponseType(typeof(StoreOfferResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreOfferResponse>> Update(
        Guid id, [FromBody] UpdateStoreOfferCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #109
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteStoreOfferCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // activate/deactivate
    [ProducesResponseType(typeof(StoreOfferResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreOfferResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateStoreOfferStatusCommand(id, request.Status), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/business-accounts")]
public sealed class AdminBusinessAccountsController : ApiControllerBase
{
    public AdminBusinessAccountsController(ISender sender) : base(sender) { }

    [HttpGet] // #110
    [ProducesResponseType(typeof(PagedResult<BusinessAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BusinessAccountResponse>>> List(
        [FromQuery] GetBusinessAccountsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminBusinessAccount")] // #111
    [ProducesResponseType(typeof(BusinessAccountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessAccountResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetBusinessAccountByIdQuery(id), ct));

    [HttpPost] // #112
    [ProducesResponseType(typeof(BusinessAccountResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<BusinessAccountResponse>> Create(
        [FromBody] CreateBusinessAccountCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminBusinessAccount", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #113
    [ProducesResponseType(typeof(BusinessAccountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessAccountResponse>> Update(
        Guid id, [FromBody] UpdateBusinessAccountCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #114
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteBusinessAccountCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #115 approve/block
    [ProducesResponseType(typeof(BusinessAccountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessAccountResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateBusinessAccountStatusCommand(id, request.Status), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/agent-onboarding")]
public sealed class AdminAgentOnboardingController : ApiControllerBase
{
    public AdminAgentOnboardingController(ISender sender) : base(sender) { }

    [HttpGet] // #116
    [ProducesResponseType(typeof(PagedResult<AgentOnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AgentOnboardingResponse>>> List(
        [FromQuery] GetAgentOnboardingQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminAgentOnboarding")] // #117
    [ProducesResponseType(typeof(AgentOnboardingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentOnboardingResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetAgentOnboardingByIdQuery(id), ct));

    [HttpPost] // #118
    [ProducesResponseType(typeof(AgentOnboardingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentOnboardingResponse>> Create(
        [FromBody] CreateAgentOnboardingCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminAgentOnboarding", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #119
    [ProducesResponseType(typeof(AgentOnboardingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentOnboardingResponse>> Update(
        Guid id, [FromBody] UpdateAgentOnboardingCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #120
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteAgentOnboardingCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #121 approve/reject
    [ProducesResponseType(typeof(AgentOnboardingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentOnboardingResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateAgentOnboardingStatusCommand(id, request.Status), ct));
}
