using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ECommercePlatform.Application.Common.Messaging;

/// <summary>
/// The mediator. A request's concrete type is only known at runtime, so the first
/// dispatch of each type builds a closed-generic wrapper and caches it; every
/// later dispatch is a dictionary lookup and a virtual call, with no further
/// reflection.
/// </summary>
public sealed class Sender : ISender
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            static (requestType, responseType) =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>)
                    .MakeGenericType(requestType, responseType);

                return Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException(
                        $"Could not construct a dispatcher for '{requestType.Name}'.");
            },
            typeof(TResponse));

        return wrapper.Handle(request, _serviceProvider, cancellationToken);
    }

    private abstract class RequestHandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> Handle(
            object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    private sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(
            object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;

            var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
                ?? throw new InvalidOperationException(
                    $"No handler is registered for '{typeof(TRequest).Name}'. Every request needs "
                    + $"exactly one IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}>.");

            RequestHandlerDelegate<TResponse> next =
                ct => handler.Handle(typedRequest, ct);

            // Behaviors are resolved in registration order, so composing them in
            // reverse leaves the first-registered one outermost.
            var behaviors = serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse();

            foreach (var behavior in behaviors)
            {
                var nextStage = next;
                next = ct => behavior.Handle(typedRequest, nextStage, ct);
            }

            return next(cancellationToken);
        }
    }
}
