using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Reviews;
using ECommercePlatform.Application.Features.Shop.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/reviews")]
public sealed class StorefrontReviewsController : ApiControllerBase
{
    private readonly ILogger<StorefrontReviewsController> _logger;
    public StorefrontReviewsController(ISender sender, ILogger<StorefrontReviewsController> logger) : base(sender) => _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> List(
        [FromQuery] string? productName, [FromQuery] int count = 25, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("List action started.");
        try { return ToResponse(await Sender.Send(new GetActiveReviewsQuery(productName, count), cancellationToken)); }
        finally { _logger.LogInformation("List action finished."); }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] CreateReviewCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");
        try
        {
            var result = await Sender.Send(command, cancellationToken);
            return result.IsFailure ? ToProblem(result.Error!) : CreatedAtAction(nameof(List), new {}, result.Value);
        }
        finally { _logger.LogInformation("Create action finished."); }
    }
}
