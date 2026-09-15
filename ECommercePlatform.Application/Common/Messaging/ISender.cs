namespace ECommercePlatform.Application.Common.Messaging;

/// <summary>
/// Dispatches a request to its handler through the behavior pipeline. Controllers
/// depend on this alone, so they never reference a handler or a service directly.
/// </summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
