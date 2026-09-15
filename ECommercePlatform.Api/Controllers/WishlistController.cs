using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Wishlist.AddToWishlist;
using ECommercePlatform.Application.Features.Wishlist.Dtos;
using ECommercePlatform.Application.Features.Wishlist.GetMyWishlist;
using ECommercePlatform.Application.Features.Wishlist.RemoveWishlistItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;
using ECommercePlatform.Application.Features.Wishlist;
/// <summary>
/// The signed-in caller's own wishlist. Same authorization shape as CartController:
/// authentication is the only check, no permission gate.
/// </summary>
[Authorize]
[Route("api/v1/wishlist")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class WishlistController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public WishlistController(ISender sender, ICurrentUser currentUser) : base(sender)
        => _currentUser = currentUser;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WishlistItemResponse>>> Get(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(WishlistErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new GetMyWishlistQuery(userId), cancellationToken);

        return ToResponse(result);
    }

    [HttpPost("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Add(Guid productId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(WishlistErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new AddToWishlistCommand(userId, productId), cancellationToken);

        return ToNoContent(result);
    }

    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remove(Guid productId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(WishlistErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new RemoveWishlistItemCommand(userId, productId), cancellationToken);

        return ToNoContent(result);
    }
}