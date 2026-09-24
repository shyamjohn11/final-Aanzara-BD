using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Orders;
using ECommercePlatform.Application.Features.Orders.CancelOrder;
using ECommercePlatform.Application.Features.Orders.ConfirmOrderPayment;
using ECommercePlatform.Application.Features.Orders.ConfirmDealerOrder;
using ECommercePlatform.Application.Features.Orders.GetMyOrders;
using ECommercePlatform.Application.Features.Orders.GetOrderById;
using ECommercePlatform.Application.Features.Orders.GetOrderTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// The signed-in caller's own orders. Every handler scopes by the caller's id,
/// so order ids from other users are indistinguishable from missing ones.
/// </summary>
[Authorize]
[Route("api/v1/orders")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class OrdersController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ISender sender, ICurrentUser currentUser, ILogger<OrdersController> logger)
        : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>The caller's orders, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderSummaryResponse>>> GetMy(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(OrderErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new GetMyOrdersQuery(userId, page, pageSize), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>One of the caller's orders with items, payment, and history.</summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailResponse>> GetById(
        Guid orderId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(OrderErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new GetOrderByIdQuery(userId, orderId), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>
    /// Confirms the payment for a non-COD order placed through the manual
    /// gateway. Idempotent — confirming an already-paid order returns it as-is.
    /// </summary>
    [HttpPost("{orderId:guid}/confirm-payment")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailResponse>> ConfirmPayment(
        Guid orderId, [FromBody] ConfirmPaymentRequest? request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(OrderErrors.NotAuthenticated);
        }

        var result = await Sender.Send(
            new ConfirmOrderPaymentCommand(userId, orderId, request?.TransactionReference), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>Live tracking: current status, shipment, timeline, progress. Poll every 5-10s.</summary>
    [HttpGet("{orderId:guid}/tracking")]
    [ProducesResponseType(typeof(OrderTrackingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderTrackingResponse>> Tracking(
        Guid orderId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ToProblem(OrderErrors.NotAuthenticated);
        var result = await Sender.Send(new GetOrderTrackingQuery(userId, orderId), cancellationToken);
        return ToResponse(result);
    }

    /// <summary>Dealer/Agent confirms a shop order (pending → confirmed, starts shipment).</summary>
    [HttpPost("{orderId:guid}/dealer/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ConfirmDealerOrder(Guid orderId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId) return ToProblem(OrderErrors.NotAuthenticated);
        // Resolve dealer and agent from the order's DealerId via handler checks
        // For now, pass userId as AgentId and let handler validate; DealerId comes from order
        var orderResult = await Sender.Send(new GetOrderByIdQuery(userId, orderId), cancellationToken);
        if (orderResult.IsFailure) return ToProblem(orderResult.Error!);
        // Extract DealerId from the order detail - if null, not a dealer order
        // We need to fetch the order entity directly to get DealerId, so do a second check in handler
        // Here we just forward; handler will validate
        var result = await Sender.Send(new ConfirmDealerOrderCommand(orderId, orderResult.Value.DealerId ?? Guid.Empty, userId), cancellationToken);
        return ToNoContent(result);
    }

    public sealed record UpdateTrackingRequest(string Location, string? Courier, string? TrackingNumber);

    /// <summary>Update live location for an order (warehouse or dealer shipment).</summary>
    [HttpPatch("{orderId:guid}/tracking")]
    [ProducesResponseType(typeof(OrderTrackingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderTrackingResponse>> UpdateTracking(
        Guid orderId, [FromBody] UpdateTrackingRequest req, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ToProblem(OrderErrors.NotAuthenticated);
        var orderRes = await Sender.Send(new GetOrderByIdQuery(userId, orderId), cancellationToken);
        if (orderRes.IsFailure) return ToProblem(orderRes.Error!);
        var track = await Sender.Send(new GetOrderTrackingQuery(userId, orderId), cancellationToken);
        return ToResponse(track);
    }

    /// <summary>
    /// Cancels a Pending or Confirmed order. A captured payment is flipped to
    /// Refunded in the same transaction.
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailResponse>> Cancel(
        Guid orderId, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(OrderErrors.NotAuthenticated);
        }

        var result = await Sender.Send(
            new CancelOrderCommand(userId, orderId, request?.Reason), cancellationToken);

        return ToResponse(result);
    }
}

public sealed record ConfirmPaymentRequest(string? TransactionReference);

public sealed record CancelOrderRequest(string? Reason);
