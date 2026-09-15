namespace ECommercePlatform.Infrastructure.Services.Email;

/// <summary>
/// SMTP configuration bound from the "Email" section in appsettings.json.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string? Username { get; set; }

    public string? AppPassword { get; set; }

    public string SenderEmail { get; set; } = "noreply@ecommerceplatform.local";

    public string SenderName { get; set; } = "ECommercePlatform";
}
