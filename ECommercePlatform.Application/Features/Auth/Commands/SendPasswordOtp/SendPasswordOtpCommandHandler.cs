using System.Security.Cryptography;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.SendPasswordOtp;

public sealed class SendPasswordOtpCommandHandler : ICommandHandler<SendPasswordOtpCommand, Result>
{
    private const string OtpPurpose = "PASSWORD_RESET";
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);

    private readonly IUserRepository _users;
    private readonly IOtpStore _otpStore;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendPasswordOtpCommandHandler> _logger;

    public SendPasswordOtpCommandHandler(
        IUserRepository users,
        IOtpStore otpStore,
        IEmailService emailService,
        ILogger<SendPasswordOtpCommandHandler> logger)
    {
        _users = users;
        _otpStore = otpStore;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SendPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("Password OTP requested for unregistered email {Email}.", email);
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // Generate a cryptographically secure 6-digit numeric OTP (100000 - 999999).
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        await _otpStore.SetOtpAsync(user.Email, OtpPurpose, otp, OtpLifetime, cancellationToken);

        var subject = "Your HarborSure Password Change OTP";
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Password Change Verification</title>
            </head>
            <body style="margin: 0; padding: 24px 12px; background-color: #f4f7f6; font-family: Arial, Helvetica, sans-serif; color: #24302b;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-collapse: collapse;">
                    <tr>
                        <td style="padding: 28px 36px; background-color: #0A7832; color: #ffffff;">
                            <div style="font-size: 24px; font-weight: 700; letter-spacing: 0.2px;">HarborSure</div>
                            <div style="margin-top: 6px; font-size: 14px; color: #d8f1e2;">Secure account verification</div>
                        </td>
                    </tr>
                    <tr>
                        <td style="height: 5px; background-color: #04377F; font-size: 0; line-height: 0;">&nbsp;</td>
                    </tr>
                    <tr>
                        <td style="padding: 32px 36px; font-size: 16px; line-height: 1.6;">
                            <p style="margin: 0 0 16px;">Hello {user.Name},</p>
                            <p style="margin: 0 0 24px;">You requested a verification code to change your account password. Use this one-time password (OTP) to continue:</p>
                            <div style="margin: 0 0 24px; padding: 18px; border: 1px solid #cfe5d7; border-radius: 8px; background-color: #f2faf5; text-align: center;">
                                <span style="font-size: 32px; font-weight: 700; letter-spacing: 6px; color: #04377F;">{otp}</span>
                            </div>
                            <p style="margin: 0; color: #52645c; font-size: 14px;">This code expires in <strong>10 minutes</strong>. If you did not request this password change, you can safely ignore this email.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 20px 36px; background-color: #04377F; color: #dce8f8; font-size: 12px; line-height: 1.5;">
                            This is an automated HarborSure security message. Please do not reply to this email.
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        await _emailService.SendEmailAsync(user.Email, subject, htmlBody, cancellationToken);

        _logger.LogInformation("Password change OTP issued and dispatched to {Email}.", email);

        return Result.Success();
    }
}
