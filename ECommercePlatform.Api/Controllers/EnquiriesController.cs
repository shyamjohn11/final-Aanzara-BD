using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Enquiries;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Public storefront contact inbox. No login required: anyone can send a
/// message. Submissions land in the Enquiries table with status Open and
/// show up in /admin/enquiries (plus the admin notification bell).
/// Enquiry management stays on the admin-only AdminEnquiriesController.
/// </summary>
[AllowAnonymous]
[Route("api/v1/enquiries")]
public sealed class EnquiriesController : ApiControllerBase
{
    private readonly ILogger<EnquiriesController> _logger;

    public EnquiriesController(ISender sender, ILogger<EnquiriesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Submits a contact message (always stored as Open).</summary>
    [HttpPost]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(EnquiryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnquiryResponse>> Submit(
        [FromBody] CreateEnquiryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enquiry submit started.");

        try
        {
            var name = (command.Name ?? string.Empty).Trim();
            var subject = (command.Subject ?? string.Empty).Trim();
            var message = (command.Message ?? string.Empty).Trim();

            if (name.Length == 0 || subject.Length == 0 || message.Length == 0)
            {
                return ToProblem(Error.Validation(
                    "enquiry.missing_fields",
                    "Please share your name, a subject, and your message."));
            }

            // Status is always server-set: callers cannot file pre-resolved messages.
            var result = await Sender.Send(
                command with { Name = name, Subject = subject, Message = message, Status = "Open" },
                cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Enquiry submit finished.");
        }
    }
}
