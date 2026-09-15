namespace ECommercePlatform.Application.Common.Messaging;

/// <summary>Calls the next stage of the pipeline, ending at the handler.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Wraps every request, innermost-last, so cross-cutting concerns live in one
/// place instead of being repeated at the top of each handler. Behaviors run in
/// registration order.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
