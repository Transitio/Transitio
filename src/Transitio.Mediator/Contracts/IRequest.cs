namespace Transitio.Mediator;

/// <summary>
/// Marker for a request that produces a <typeparamref name="TResponse"/> when sent through
/// <see cref="ISender.Send{TResponse}"/>. Exactly one <see cref="IRequestHandler{TRequest,TResponse}"/>
/// handles it.
/// </summary>
/// <typeparam name="TResponse">The type returned by the handler.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Marker for a request that produces no value. It is an <see cref="IRequest{TResponse}"/> of
/// <see cref="Unit"/> so the same handler and pipeline machinery serves both void and
/// value-returning requests.
/// </summary>
public interface IRequest : IRequest<Unit>
{
}
