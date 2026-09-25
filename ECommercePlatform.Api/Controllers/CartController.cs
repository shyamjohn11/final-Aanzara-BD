using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Cart;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Application.Features.Cart.GetCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;
using ECommercePlatform.Application.Features.Cart.AddToCart;
using ECommercePlatform.Application.Features.Cart.ApplyCoupon;
using ECommercePlatform.Application.Features.Cart.ClearCart;
using ECommercePlatform.Application.Features.Cart.RemoveCartItem;
using ECommercePlatform.Application.Features.Cart.UpdateCartQuantity;
/// <summary>
/// The signed-in caller's own cart. There is no permission gate here the way
/// there is on the admin catalog controllers — every authenticated user may read
/// and act on their own cart, so authentication is the only check.
/// </summary>
[Authorize]
[Route("api/v1/cart")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class CartController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CartController> _logger;

    public CartController(ISender sender, ICurrentUser currentUser, ILogger<CartController> logger)
        : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// The caller's cart: line items plus an embedded summary (subtotal, tax,
    /// delivery estimate, total). An empty cart is 200 with no items, not 404.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(CartErrors.NotAuthenticated);
            }

            var result = await Sender.Send(new GetCartQuery(userId), cancellationToken);

            return ToResponse(result);
        }
        catch (OperationCanceledException) when (ClientWentAway())
        {
            // Browser navigated away mid-request (499 = Client Closed
            // Request). Handled here in user code so the debugger does
            // not break on the cancellation; the edge already dropped
            // the connection, so nothing is written back.
            return StatusCode(499);
        }
        finally
        {
            _logger.LogInformation("Get action finished.");
        }
    }

    [HttpPost("items")]
[ProducesResponseType(typeof(CartItemResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<CartItemResponse>> AddItem(
    [FromBody] AddToCartRequest request, CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(
            new AddToCartCommand(userId, request.ProductId, request.Quantity), cancellationToken);

        return ToResponse(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}

[HttpPatch("items/{cartItemId:guid}")]
[ProducesResponseType(typeof(CartItemResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<CartItemResponse>> UpdateItemQuantity(
    Guid cartItemId, [FromBody] UpdateCartQuantityRequest request, CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(
            new UpdateCartQuantityCommand(userId, cartItemId, request.Quantity), cancellationToken);

        return ToResponse(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}

/// <summary>
/// Removes one item. The recalculated cart (GST included) is what GET returns —
/// delete responses stay payloadless per the module's convention.
/// </summary>
[HttpDelete("items/{cartItemId:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(
            new RemoveCartItemCommand(userId, cartItemId), cancellationToken);

        return ToNoContent(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}

/// <summary>Empties the caller's cart. The cart row itself is kept for reuse.</summary>
[HttpDelete("clear")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<ActionResult> Clear(CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(new ClearCartCommand(userId), cancellationToken);

        return ToNoContent(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}

/// <summary>
/// Applies a coupon code to the caller's cart and returns the recalculated summary
/// (discount + total). Invalid/expired/min-order codes fail with a problem detail.
/// </summary>
[HttpPost("coupon")]
[ProducesResponseType(typeof(CartSummaryResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<CartSummaryResponse>> ApplyCoupon(
    [FromBody] ApplyCouponRequest request, CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(
            new ApplyCouponCommand(userId, request?.Code ?? string.Empty), cancellationToken);

        return ToResponse(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}

/// <summary>Clears any coupon on the caller's cart and returns the recalculated summary.</summary>
[HttpDelete("coupon")]
[ProducesResponseType(typeof(CartSummaryResponse), StatusCodes.Status200OK)]
public async Task<ActionResult<CartSummaryResponse>> RemoveCoupon(CancellationToken cancellationToken)
{
    if (_currentUser.UserId is not { } userId)
    {
        return ToProblem(CartErrors.NotAuthenticated);
    }

    try
    {
        var result = await Sender.Send(new RemoveCouponCommand(userId), cancellationToken);

        return ToResponse(result);
    }
    catch (OperationCanceledException) when (ClientWentAway())
    {
        // Browser navigated away mid-request (499 = Client Closed Request).
        return StatusCode(499);
    }
}
}

public sealed record ApplyCouponRequest(string? Code);