using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Notifications;
using ECommercePlatform.Application.Features.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// The signed-in caller's own notification feed, derived from their order
/// history. Read-only; clients track read state locally.
/// </summary>
[Authorize]
[Route("api/v1/notifications")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class CustomerNotificationsController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public CustomerNotificationsController(ISender sender, ICurrentUser currentUser)
        : base(sender)
    {
        _currentUser = currentUser;
    }

    /// <summary>Recent order events for the caller, newest first.</summary>
    [HttpGet] // #159 ?count=
    [ProducesResponseType(typeof(IReadOnlyList<CustomerNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerNotificationResponse>>> GetMy(
        [FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(OrderErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new GetMyNotificationsQuery(userId, count), cancellationToken);

        return ToResponse(result);
    }
}
