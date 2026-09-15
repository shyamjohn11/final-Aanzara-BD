namespace ECommercePlatform.Api.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public const string PolicyName = "DefaultCorsPolicy";

    public string[] AllowedOrigins { get; set; } = [];
}
