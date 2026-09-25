using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.PricingRequests;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Public storefront bulk-quote inbox. No login required: anyone can ask
/// for wholesale pricing. Submissions land in the PricingRequests table
/// with status Pending and show up in /admin/pricing-requests (plus the
/// admin notification bell). Quote management stays on the admin-only
/// AdminPricingRequestsController.
/// </summary>
[AllowAnonymous]
[Route("api/v1/quote-requests")]
public sealed class QuoteRequestsController : ApiControllerBase
{
    private readonly ILogger<QuoteRequestsController> _logger;

    public QuoteRequestsController(ISender sender, ILogger<QuoteRequestsController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Submits a bulk-quote request (always stored as Pending).</summary>
    [HttpPost]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(PricingRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PricingRequestResponse>> Submit(
        [FromBody] CreatePricingRequestCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Quote request submit started.");

        try
        {
            var name = (command.CustomerName ?? command.Name ?? string.Empty).Trim();
            var product = (command.Product ?? string.Empty).Trim();

            if (name.Length == 0 || product.Length == 0)
            {
                return ToProblem(Error.Validation(
                    "quote.missing_fields",
                    "Please share your name and the product you need a quote for."));
            }

            if (command.Quantity <= 0)
            {
                return ToProblem(Error.Validation(
                    "quote.quantity_invalid",
                    "Quantity must be at least 1."));
            }

            // Status is always server-set: callers cannot file pre-approved quotes.
            var result = await Sender.Send(
                command with { CustomerName = name, Product = product, Status = "Pending" },
                cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Quote request submit finished.");
        }
    }
}
