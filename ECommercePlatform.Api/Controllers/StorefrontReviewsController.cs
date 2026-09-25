using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Admin.Reviews;
using ECommercePlatform.Application.Features.Shop.Reviews;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/reviews")]
public sealed class StorefrontReviewsController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<StorefrontReviewsController> _logger;
    public StorefrontReviewsController(ISender sender, ICurrentUser currentUser, ILogger<StorefrontReviewsController> logger) : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> List(
        [FromQuery] string? productName, [FromQuery] int count = 25, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("List action started.");
        try { return ToResponse(await Sender.Send(new GetActiveReviewsQuery(productName, count), cancellationToken)); }
        finally { _logger.LogInformation("List action finished."); }
    }

    /// <summary>
    /// Submits a product review. Login required, and only customers who
    /// bought the product (non-cancelled order) are accepted — everyone
    /// else gets 403. Accepted reviews stay Pending for moderation and
    /// are flagged as verified purchases for the storefront badge.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] SubmitReviewCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");
        try
        {
            // Identity comes from the token, never the body: callers cannot
            // review as someone else.
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(Error.Unauthorized(
                    "review.unauthenticated", "Please sign in to write a review."));
            }

            var result = await Sender.Send(command with { UserId = userId }, cancellationToken);
            return result.IsFailure ? ToProblem(result.Error!) : CreatedAtAction(nameof(List), new {}, result.Value);
        }
        finally { _logger.LogInformation("Create action finished."); }
    }
}
