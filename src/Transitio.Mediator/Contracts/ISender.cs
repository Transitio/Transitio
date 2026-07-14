using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Sends requests to their single handler, running any registered pipeline behaviors first.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a request to its <see cref="IRequestHandler{TRequest,TResponse}"/> and returns the response.
    /// </summary>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>Sends a void request to its handler.</summary>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
