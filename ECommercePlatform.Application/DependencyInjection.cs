using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Application;

/// <summary>
/// The Application layer's own composition root. Each layer registers what it
/// owns, so Program.cs never has to know the internals of any of them.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SessionSettings>()
            .Bind(configuration.GetSection(SessionSettings.SectionName))
            .ValidateOnStart();

        services.AddScoped<ISessionManager, SessionManager>();

        // Discovers every IRequestHandler in this assembly and installs the
        // behavior pipeline, so a new use case needs no registration of its own.
        services.AddMessaging(typeof(DependencyInjection).Assembly);

        return services;
    }
}
