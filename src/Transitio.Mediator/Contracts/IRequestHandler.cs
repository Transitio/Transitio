using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Handles a request of type <typeparamref name="TRequest"/> and returns a
/// <typeparamref name="TResponse"/>. Register one handler per request type.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Handles the request and returns its response.</summary>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a void request (<see cref="IRequest"/>). Implement only
/// <see cref="Handle(TRequest, CancellationToken)"/>; a default implementation adapts it to the
/// <see cref="Unit"/> response used by the dispatch pipeline.
/// </summary>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest
{
    /// <summary>Handles the request.</summary>
    new Task Handle(TRequest request, CancellationToken cancellationToken);

    /// <summary>Adapts the void handler to the <see cref="Unit"/>-returning pipeline.</summary>
    async Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
    {
        await Handle(request, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
