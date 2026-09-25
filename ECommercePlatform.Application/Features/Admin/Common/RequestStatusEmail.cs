using System.Net;
using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Admin.Common;

// Best-effort user email for admin status changes on enquiries and pricing
// (bulk quote) requests — mirrors OrderEmailSender for orders. SMTP failure
// is logged and swallowed so email can never roll back the saved status.
internal static class RequestStatusEmail
{
    internal static async Task TrySendAsync(
        IEmailService emailService,
        ILogger logger,
        string? recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            logger.LogWarning("Status email '{Subject}' skipped: request has no email address.", subject);
            return;
        }

        var to = recipientEmail.Trim();

        try
        {
            await emailService.SendEmailAsync(to, subject, htmlBody, cancellationToken);
            logger.LogInformation("Status email '{Subject}' dispatched to {Email}.", subject, to);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Status email '{Subject}' to {Email} failed.", subject, to);
        }
    }

    internal static string EnquiryHtml(string customerName, string subject, string status)
    {
        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Your enquiry <strong>{Html(subject)}</strong> has moved to <strong>{Html(status)}</strong>.</p>
            <p style="margin: 0; font-size: 14px; color: #52645c;">You can follow further updates from your account notifications.</p>
            """;

        return Wrap($"Enquiry update: {subject} is now {status}", "Enquiry update", body);
    }

    internal static string PricingHtml(string customerName, string product, int quantity, string status)
    {
        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Your bulk quote request for <strong>{Html(product)}</strong> &times; {quantity} has moved to <strong>{Html(status)}</strong>.</p>
            <p style="margin: 0; font-size: 14px; color: #52645c;">You can follow further updates from your account notifications.</p>
            """;

        return Wrap($"Bulk quote update: {product} is now {status}", "Bulk quote update", body);
    }

    private static string Wrap(string title, string heading, string bodyInner)
        => $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{Html(title)}</title>
        </head>
        <body style="margin: 0; padding: 24px 12px; background-color: #f4f7f6; font-family: Arial, Helvetica, sans-serif; color: #24302b;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-collapse: collapse;">
                <tr>
                    <td style="padding: 28px 36px; background-color: #0A7832; color: #ffffff;">
                        <div style="font-size: 24px; font-weight: 700; letter-spacing: 0.2px;">Aanzara</div>
                        <div style="margin-top: 6px; font-size: 14px; color: #d8f1e2;">{Html(heading)}</div>
                    </td>
                </tr>
                <tr>
                    <td style="height: 5px; background-color: #04377F; font-size: 0; line-height: 0;">&nbsp;</td>
                </tr>
                <tr>
                    <td style="padding: 32px 36px; font-size: 16px; line-height: 1.6;">
                        {bodyInner}
                    </td>
                </tr>
                <tr>
                    <td style="padding: 20px 36px; background-color: #04377F; color: #dce8f8; font-size: 12px; line-height: 1.5;">
                        This is an automated Aanzara message. Please do not reply to this email.
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
