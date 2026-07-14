using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Transitio.Mediator;

namespace Transitio.Mediator.Tests;

// Records entry/exit order around the handler so tests can assert pipeline ordering.
public sealed class TraceBehavior : IPipelineBehavior<Ping, string>
{
    private readonly PipelineTrace _trace;
    private readonly string _name;

    public TraceBehavior(PipelineTrace trace, string name)
    {
        _trace = trace;
        _name = name;
    }

    public async Task<string> Handle(Ping request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        _trace.Entries.Add($"{_name}:before");
        var response = await next();
        _trace.Entries.Add($"{_name}:after");
        return response;
    }
}

// Short-circuits the pipeline by not calling next().
public sealed class ShortCircuitBehavior : IPipelineBehavior<Ping, string>
{
    public Task<string> Handle(Ping request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        => Task.FromResult("short-circuited");
}

// Rewrites the handler's response, proving next()'s return value propagates.
public sealed class UppercaseBehavior : IPipelineBehavior<Ping, string>
{
    public async Task<string> Handle(Ping request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return response.ToUpperInvariant();
    }
}

// A behavior over a void (Unit-returning) request, used to prove behaviors wrap void requests too.
public sealed class VoidTraceBehavior : IPipelineBehavior<DoWork, Unit>
{
    private readonly PipelineTrace _trace;

    public VoidTraceBehavior(PipelineTrace trace) => _trace = trace;

    public async Task<Unit> Handle(DoWork request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
    {
        _trace.Entries.Add("void:before");
        var response = await next();
        _trace.Entries.Add("void:after");
        return response;
    }
}

public sealed class PipelineTrace
{
    public List<string> Entries { get; } = new();
}
