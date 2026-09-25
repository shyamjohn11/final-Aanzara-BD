namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Server-side payment confirmation checks. The browser never decides Success.
/// </summary>
public interface IPaymentVerificationService
{
    /// <summary>
    /// Verifies a client-provided gateway payload (e.g. Razorpay signature).
    /// Returns false when verification fails or the gateway is not configured.
    /// </summary>
    Task<bool> VerifyGatewayConfirmationAsync(
        string? orderId,
        string? gatewayPaymentId,
        string? signature,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True only when configuration explicitly allows unverified Manual confirm
    /// (dev/demo). Production default is false.
    /// </summary>
    bool AllowManualClientConfirm { get; }
}
