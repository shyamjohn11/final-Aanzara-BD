using System.Net;
using System.Text;
using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Orders;

// Order-module-only email helper. Keeps every order write handler to a
// two-line change (inject IEmailService + ILogger, then one TrySend call)
// with zero changes to controllers, DTOs, repositories, DbContext, or DI:
// IEmailService is already registered application-wide.
internal sealed record OrderEmailLine(string ProductName, int Quantity, decimal UnitPrice);

internal static class OrderEmailSender
{
    // Best-effort dispatch: SMTP failure is logged and swallowed so email
    // can never roll back an already-saved order or fail checkout.
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
            logger.LogWarning("Order email '{Subject}' skipped: order user has no email address.", subject);
            return;
        }

        var to = recipientEmail.Trim();

        try
        {
            await emailService.SendEmailAsync(to, subject, htmlBody, cancellationToken);
            logger.LogInformation("Order email '{Subject}' dispatched to {Email}.", subject, to);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Order email '{Subject}' to {Email} failed.", subject, to);
        }
    }

    internal static string PlaceOrderHtml(
        string customerName,
        string orderNo,
        IReadOnlyList<OrderEmailLine> lines,
        decimal subtotal,
        decimal discount,
        decimal gst,
        decimal delivery,
        decimal handling,
        decimal total,
        string? couponCode,
        string paymentMethod,
        string paymentStatus,
        string shipTo,
        string addressSummary)
    {
        var rows = new StringBuilder();
        foreach (var line in lines)
        {
            rows.Append("<tr>")
                .Append($"<td style=\"padding: 8px 12px; border-bottom: 1px solid #e5ece8;\">{Html(line.ProductName)}</td>")
                .Append($"<td style=\"padding: 8px 12px; border-bottom: 1px solid #e5ece8; text-align: center;\">{line.Quantity}</td>")
                .Append($"<td style=\"padding: 8px 12px; border-bottom: 1px solid #e5ece8; text-align: right;\">₹{line.UnitPrice:0.00}</td>")
                .Append($"<td style=\"padding: 8px 12px; border-bottom: 1px solid #e5ece8; text-align: right;\">₹{line.UnitPrice * line.Quantity:0.00}</td>")
                .Append("</tr>");
        }

        var totals = new StringBuilder()
            .Append(TotalRow("Subtotal", subtotal));
        if (discount > 0)
        {
            totals.Append(TotalRow("Discount" + (couponCode is null ? string.Empty : $" ({Html(couponCode)})"), -discount));
        }
        totals
            .Append(TotalRow("GST", gst))
            .Append(TotalRow("Delivery", delivery))
            .Append(TotalRow("Handling", handling))
            .Append($"<tr><td colspan=\"3\" style=\"padding: 10px 12px; font-weight: 700;\">Grand total</td><td style=\"padding: 10px 12px; text-align: right; font-weight: 700;\">₹{total:0.00}</td></tr>");

        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Thank you for your order. Your order <strong>{Html(orderNo)}</strong> has been placed successfully and is now <strong>Pending</strong>.</p>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="border-collapse: collapse; margin: 0 0 16px; font-size: 14px;">
                <tr style="background-color: #f2faf5;">
                    <th style="padding: 8px 12px; text-align: left;">Item</th>
                    <th style="padding: 8px 12px; text-align: center;">Qty</th>
                    <th style="padding: 8px 12px; text-align: right;">Price</th>
                    <th style="padding: 8px 12px; text-align: right;">Total</th>
                </tr>
                {rows}
            </table>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="border-collapse: collapse; margin: 0 0 16px; font-size: 14px;">
                {totals}
            </table>
            <p style="margin: 0 0 8px; font-size: 14px;">Payment: <strong>{Html(paymentMethod)}</strong> ({Html(paymentStatus)})</p>
            <p style="margin: 0 0 8px; font-size: 14px;">Deliver to: <strong>{Html(shipTo)}</strong></p>
            <p style="margin: 0; font-size: 14px; color: #52645c;">{Html(addressSummary)}</p>
            """;

        return Wrap($"Order {orderNo} confirmed", "Order confirmed", body);
    }

    internal static string CancelHtml(
        string customerName,
        string orderNo,
        string? reason,
        bool refunded)
    {
        var refundNote = refunded
            ? "<p style=\"margin: 0 0 16px; font-size: 14px;\">The captured payment has been marked for refund.</p>"
            : string.Empty;
        var reasonNote = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $"<p style=\"margin: 0 0 16px; font-size: 14px;\">Reason: {Html(reason.Trim())}</p>";

        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Your order <strong>{Html(orderNo)}</strong> has been <strong>cancelled</strong>.</p>
            {reasonNote}
            {refundNote}
            """;

        return Wrap($"Order {orderNo} cancelled", "Order cancelled", body);
    }

    internal static string PaymentConfirmedHtml(
        string customerName,
        string orderNo,
        decimal amount,
        string paymentMethod,
        string? transactionReference)
    {
        var txnNote = string.IsNullOrWhiteSpace(transactionReference)
            ? string.Empty
            : $"<p style=\"margin: 0 0 16px; font-size: 14px;\">Transaction reference: {Html(transactionReference.Trim())}</p>";

        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Payment of <strong>₹{amount:0.00}</strong> via <strong>{Html(paymentMethod)}</strong> for order <strong>{Html(orderNo)}</strong> has been confirmed. Your order is now being processed.</p>
            {txnNote}
            """;

        return Wrap($"Payment confirmed for order {orderNo}", "Payment confirmed", body);
    }

    internal static string StatusChangedHtml(
        string customerName,
        string orderNo,
        string status)
    {
        var body = $"""
            <p style="margin: 0 0 16px;">Hello {Html(customerName)},</p>
            <p style="margin: 0 0 16px;">Your order <strong>{Html(orderNo)}</strong> has moved to <strong>{Html(status)}</strong>.</p>
            <p style="margin: 0; font-size: 14px; color: #52645c;">You can track the latest status from your orders page.</p>
            """;

        return Wrap($"Order {orderNo} is now {status}", "Order update", body);
    }

    private static string TotalRow(string label, decimal value)
        => $"<tr><td colspan=\"3\" style=\"padding: 6px 12px; color: #52645c;\">{label}</td><td style=\"padding: 6px 12px; text-align: right;\">₹{value:0.00}</td></tr>";

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
                        This is an automated Aanzara order message. Please do not reply to this email.
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
