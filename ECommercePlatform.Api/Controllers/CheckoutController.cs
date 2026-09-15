using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Orders;
using ECommercePlatform.Application.Features.Orders.PlaceOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Turns the caller's cart into an order. Like the cart endpoints, there is no
/// permission gate — authentication is the only check; the address id and cart
/// are both scoped to the caller server-side.
/// </summary>
[Authorize]
[Route("api/v1/checkout")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class CheckoutController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ISender sender, ICurrentUser currentUser, ILogger<CheckoutController> logger)
        : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Places an order from the caller's current cart against one of their saved
    /// addresses. Totals are recomputed server-side from the same pricing engine
    /// the cart page uses, so posted amounts are never trusted.
    /// </summary>
    [HttpPost("place-order")]
    [ProducesResponseType(typeof(PlaceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlaceOrderResponse>> PlaceOrder(
        [FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Place order started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(OrderErrors.NotAuthenticated);
            }

            var result = await Sender.Send(
                new PlaceOrderCommand(userId, request.AddressId, request.PaymentMethod), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Place order finished.");
        }
    }
}

public sealed record PlaceOrderRequest(Guid AddressId, string PaymentMethod = "COD");
