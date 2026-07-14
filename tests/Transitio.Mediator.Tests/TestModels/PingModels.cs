using System.Threading;
using System.Threading.Tasks;
using Transitio.Mediator;

namespace Transitio.Mediator.Tests;

// A request that returns a value.
public sealed class Ping : IRequest<string>
{
    public Ping(string message) => Message = message;
    public string Message { get; }
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}

// A void request (no response).
public sealed class DoWork : IRequest
{
    public DoWork(string tag) => Tag = tag;
    public string Tag { get; }
}

public sealed class DoWorkHandler : IRequestHandler<DoWork>
{
    // Records the last handled tag so tests can assert the handler actually ran.
    public static string? LastTag;

    public Task Handle(DoWork request, CancellationToken cancellationToken)
    {
        LastTag = request.Tag;
        return Task.CompletedTask;
    }
}
