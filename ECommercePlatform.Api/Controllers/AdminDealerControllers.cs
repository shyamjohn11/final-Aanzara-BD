using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Agents;
using ECommercePlatform.Application.Features.Admin.DealerProducts;
using ECommercePlatform.Application.Features.Admin.Dealers;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/agents")]
public sealed class AdminAgentsController : ApiControllerBase
{
    public AdminAgentsController(ISender sender) : base(sender) { }

    [HttpGet] // #156
    [ProducesResponseType(typeof(PagedResult<AgentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AgentResponse>>> List(
        [FromQuery] GetAgentsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminAgent")] // #157
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetAgentByIdQuery(id), ct));

    [HttpPost] // #158 create agent (creates User + Agent + Agent role)
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentResponse>> Create(
        [FromBody] CreateAgentCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminAgent", new { id = result.Value.AgentId }, result.Value);
    }

    [HttpPut("{id:guid}")] // #159 update agent
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentResponse>> Update(
        Guid id, [FromBody] UpdateAgentCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #160 delete agent (blocked if has dealers)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteAgentCommand(id), ct));

    public sealed record UpdateAgentStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #161 agent status
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateAgentStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateAgentStatusCommand(id, request.Status), ct));

    [HttpGet("{agentId:guid}/dealers")] // #162
    [ProducesResponseType(typeof(PagedResult<DealerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DealerResponse>>> ListDealers(
        Guid agentId, [FromQuery] GetDealersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query with { AgentId = agentId }, ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/dealers")]
public sealed class AdminDealersController : ApiControllerBase
{
    public AdminDealersController(ISender sender) : base(sender) { }

    [HttpGet] // #150
    [ProducesResponseType(typeof(PagedResult<DealerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DealerResponse>>> List(
        [FromQuery] GetDealersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminDealer")] // #151
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DealerResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetDealerByIdQuery(id), ct));

    [HttpPost] // #152
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<DealerResponse>> Create(
        [FromBody] CreateDealerCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminDealer", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #153
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DealerResponse>> Update(
        Guid id, [FromBody] UpdateDealerCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #154
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteDealerCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // #155 activate/deactivate
    [ProducesResponseType(typeof(DealerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DealerResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateDealerStatusCommand(id, request.Status), ct));

    [HttpGet("{dealerId:guid}/products")] // #158
    [ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductSummaryResponse>>> ListProducts(
        Guid dealerId, [FromQuery] GetDealerProductsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query with { DealerId = dealerId }, ct));

    [HttpPost("{dealerId:guid}/products")] // #159
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        Guid dealerId, [FromBody] CreateDealerProductCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { DealerId = dealerId }, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute(
                "GetAdminDealerProduct",
                new { dealerId, productId = result.Value.ProductId },
                result.Value);
    }

    [HttpGet("{dealerId:guid}/products/{productId:guid}", Name = "GetAdminDealerProduct")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        Guid dealerId, Guid productId, CancellationToken ct)
        => ToResponse(await Sender.Send(
            new GetDealerProductByIdQuery(dealerId, productId), ct));

    [HttpPut("{dealerId:guid}/products/{productId:guid}")] // #160
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        Guid dealerId, Guid productId,
        [FromBody] UpdateDealerProductCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(
            command with { DealerId = dealerId, ProductId = productId }, ct));

    [HttpDelete("{dealerId:guid}/products/{productId:guid}")] // #161
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteProduct(
        Guid dealerId, Guid productId, CancellationToken ct)
        => ToNoContent(await Sender.Send(
            new DeleteDealerProductCommand(dealerId, productId), ct));
}

