namespace ECommercePlatform.Application.Common.Messaging;

/// <summary>
/// A message that produces a response. Handled by exactly one
/// <see cref="IRequestHandler{TRequest,TResponse}"/>.
/// </summary>
/// <remarks>
/// The generic parameter is what lets <see cref="ISender"/> infer the response
/// type at the call site, so callers never restate it.
/// </remarks>
public interface IRequest<out TResponse>;

/// <summary>
/// A request that changes state. Separating this from <see cref="IQuery{TResponse}"/>
/// is the whole point of CQRS here: the two sides get different pipeline
/// behavior, and the intent of every message is visible from its declaration.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>A request that only reads state and must leave it unchanged.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
