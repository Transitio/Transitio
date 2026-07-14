using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Continuation that invokes the next behavior in the pipeline, or the request handler itself when
/// no behaviors remain.
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Wraps request handling to add cross-cutting behavior (logging, validation, transactions, ...).
/// Behaviors run in registration order around the handler; call <paramref name="next"/> to continue
/// the pipeline or skip it to short-circuit.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Handles the request, optionally invoking <paramref name="next"/> to continue.</summary>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
