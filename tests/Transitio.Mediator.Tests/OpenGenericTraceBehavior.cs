using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;
using Xunit;
 
namespace Transitio.Mediator.Tests;
 
// An open-generic pipeline behavior. Transparent pass-through with no constructor dependencies, so
// discovering it via assembly scan cannot break unrelated tests in this assembly that also scan
// TestAssembly (e.g. DependencyInjectionTests' Echo tests) — it never changes a response and never
// requires a service those tests don't register. Static state is per closed-generic instantiation
// (OpenGenericTraceBehavior<Whisper,string> and OpenGenericTraceBehavior<Murmur,string> each get
// their own Entries queue), so request types used only in this file stay fully isolated.
public sealed class OpenGenericTraceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static ConcurrentQueue<string> Entries { get; } = new();
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Entries.Enqueue("open:before");
        var response = await next();
        Entries.Enqueue("open:after");
        return response;
    }
}
 
public sealed class Whisper : IRequest<string>
{
    public Whisper(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class WhisperHandler : IRequestHandler<Whisper, string>
{
    public Task<string> Handle(Whisper request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
// A closed behavior for Whisper only, used to prove open-generic and closed behaviors compose.
public sealed class WhisperClosedBehavior : IPipelineBehavior<Whisper, string>
{
    public async Task<string> Handle(Whisper request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        OpenGenericTraceBehavior<Whisper, string>.Entries.Enqueue("closed:before");
        var response = await next();
        OpenGenericTraceBehavior<Whisper, string>.Entries.Enqueue("closed:after");
        return response;
    }
}
 
public sealed class Murmur : IRequest<string>
{
    public Murmur(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class MurmurHandler : IRequestHandler<Murmur, string>
{
    public Task<string> Handle(Murmur request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
public sealed class Rumor : IRequest<string>
{
    public Rumor(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class RumorHandler : IRequestHandler<Rumor, string>
{
    public Task<string> Handle(Rumor request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
public sealed class Gossip : IRequest<string>
{
    public Gossip(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class GossipHandler : IRequestHandler<Gossip, string>
{
    public Task<string> Handle(Gossip request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
public sealed class Hearsay : IRequest<string>
{
    public Hearsay(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class HearsayHandler : IRequestHandler<Hearsay, string>
{
    public Task<string> Handle(Hearsay request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
// Not a genuinely open IPipelineBehavior<,>: TUnused is padded on to match the interface's arity
// (2 type parameters) without being used by it — the interface is fixed to IPipelineBehavior<TRequest,
// string>, not IPipelineBehavior<TRequest, TUnused>. A plain arity-count check can't tell this apart
// from a real IPipelineBehavior<TRequest,TResponse>; must be excluded by name/position, not count.
public sealed class PaddedArityBehavior<TRequest, TUnused> : IPipelineBehavior<TRequest, string>
    where TRequest : IRequest<string>
{
    public static int CallCount;
 
    public Task<string> Handle(TRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        CallCount++;
        return next();
    }
}
 
public sealed class Innuendo : IRequest<string>
{
    public Innuendo(string text) => Text = text;
    public string Text { get; }
}
 
public sealed class InnuendoHandler : IRequestHandler<Innuendo, string>
{
    public Task<string> Handle(Innuendo request, CancellationToken cancellationToken) => Task.FromResult(request.Text);
}
 
public class OpenGenericPipelineBehaviorTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(OpenGenericPipelineBehaviorTests).Assembly;
 
    [Fact]
    public async Task AddTransitioMediator_discovers_and_runs_open_generic_pipeline_behavior()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
 
        var result = await mediator.Send(new Murmur("hi"));
 
        Assert.Equal("hi", result);
        Assert.Equal(new[] { "open:before", "open:after" }, OpenGenericTraceBehavior<Murmur, string>.Entries.ToArray());
    }
 
    [Fact]
    public async Task Open_generic_behavior_and_closed_behavior_for_the_same_request_both_run()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
 
        var result = await mediator.Send(new Whisper("hi"));
 
        Assert.Equal("hi", result);
        // Pass 1 (closed interfaces) registers WhisperClosedBehavior before pass 2 (open generics)
        // registers OpenGenericTraceBehavior<,> — registration order determines outer-to-inner
        // wrapping, so the closed behavior wraps the open one.
        Assert.Equal(
            new[] { "closed:before", "open:before", "open:after", "closed:after" },
            OpenGenericTraceBehavior<Whisper, string>.Entries.ToArray());
    }
 
    [Fact]
    public async Task Same_open_generic_behavior_applies_independently_to_multiple_request_types()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
 
        await mediator.Send(new Rumor("a"));
        await mediator.Send(new Gossip("b"));
 
        Assert.Equal(new[] { "open:before", "open:after" }, OpenGenericTraceBehavior<Rumor, string>.Entries.ToArray());
        Assert.Equal(new[] { "open:before", "open:after" }, OpenGenericTraceBehavior<Gossip, string>.Entries.ToArray());
    }
 
    [Fact]
    public async Task Calling_AddTransitioMediator_twice_does_not_double_register_the_open_generic_behavior()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        services.AddTransitioMediator(TestAssembly); // idempotent: must not stack registrations
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
 
        await mediator.Send(new Hearsay("hi"));
 
        Assert.Equal(new[] { "open:before", "open:after" }, OpenGenericTraceBehavior<Hearsay, string>.Entries.ToArray());
    }
 
    [Fact]
    public async Task Padded_arity_behavior_is_excluded_not_registered_as_open()
    {
        // If PaddedArityBehavior<,> were wrongly treated as fully open, resolving
        // IPipelineBehavior<Innuendo,string> would construct it via MakeGenericType(Innuendo, string) —
        // substituting the closed service's own two type arguments positionally into TRequest/TUnused —
        // so it's specifically PaddedArityBehavior<Innuendo,string>.CallCount (not some other closure)
        // that would move off zero if the exclusion regressed.
        PaddedArityBehavior<Innuendo, string>.CallCount = 0;
 
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
 
        var result = await mediator.Send(new Innuendo("hi"));
 
        Assert.Equal("hi", result);
        Assert.Equal(0, PaddedArityBehavior<Innuendo, string>.CallCount);
    }
}