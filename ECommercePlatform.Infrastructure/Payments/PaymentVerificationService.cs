using System.Security.Cryptography;
using System.Text;
using ECommercePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Infrastructure.Payments;

public sealed class PaymentVerificationOptions
{
    public const string SectionName = "Payment";
    public bool AllowClientManualConfirm { get; set; }
    public RazorpayOptions Razorpay { get; set; } = new();
}

public sealed class RazorpayOptions
{
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

/// <summary>
/// Verifies Razorpay checkout signatures (HMAC-SHA256 over order_id|payment_id).
/// Manual client confirm is opt-in via Payment:AllowClientManualConfirm (default false).
/// </summary>
public sealed class PaymentVerificationService : IPaymentVerificationService
{
    private readonly PaymentVerificationOptions _options;
    private readonly ILogger<PaymentVerificationService> _logger;

    public PaymentVerificationService(
        Microsoft.Extensions.Options.IOptions<PaymentVerificationOptions> options,
        ILogger<PaymentVerificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool AllowManualClientConfirm => _options.AllowClientManualConfirm;

    public Task<bool> VerifyGatewayConfirmationAsync(
        string? orderId,
        string? gatewayPaymentId,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId)
            || string.IsNullOrWhiteSpace(gatewayPaymentId)
            || string.IsNullOrWhiteSpace(signature))
        {
            return Task.FromResult(false);
        }

        var secret = _options.Razorpay.KeySecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("Razorpay KeySecret is not configured; rejecting gateway confirmation.");
            return Task.FromResult(false);
        }

        var payload = $"{orderId}|{gatewayPaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
            .ToLowerInvariant();

        // Fixed-time compare to avoid timing side channels.
        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()));

        return Task.FromResult(valid);
    }
}
