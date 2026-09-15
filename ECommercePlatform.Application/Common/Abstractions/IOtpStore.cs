namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Stores and validates one-time passwords (OTPs) with expiration and single-use consumption.
/// </summary>
public interface IOtpStore
{
    /// <summary>
    /// Saves an OTP for a given recipient and purpose with a specific lifetime.
    /// Overwrites any unconsumed prior OTP for the same recipient and purpose.
    /// </summary>
    Task SetOtpAsync(
        string email,
        string purpose,
        string otp,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the OTP. If valid and not expired, consumes the OTP atomically so it cannot be reused.
    /// Returns true if valid, false otherwise.
    /// </summary>
    Task<bool> ValidateAndConsumeOtpAsync(
        string email,
        string purpose,
        string otp,
        CancellationToken cancellationToken = default);
}
