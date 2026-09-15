using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ECommercePlatform.Application.Common.Messaging.Behaviors;

namespace ECommercePlatform.Application.Common.Messaging;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers the mediator, every handler found in <paramref name="assembly"/>,
    /// and the behavior pipeline.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<ISender, Sender>();

        RegisterHandlers(services, assembly);

        // Order is the pipeline order, outermost first. Logging wraps validation so
        // a rejected request is still recorded with its timing.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IRequestHandler<,>);
        var registered = new Dictionary<Type, Type>();

        var candidates = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (var implementation in candidates)
        {
            var closedInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            foreach (var closedInterface in closedInterfaces)
            {
                // Two handlers for one request would mean the winner depends on
                // registration order — always a bug, so fail at start-up.
                if (registered.TryGetValue(closedInterface, out var existing))
                {
                    throw new InvalidOperationException(
                        $"'{closedInterface.GetGenericArguments()[0].Name}' has more than one handler "
                        + $"('{existing.Name}' and '{implementation.Name}'). A request must have exactly one.");
                }

                registered.Add(closedInterface, implementation);
                services.AddScoped(closedInterface, implementation);
            }
        }
    }
}
