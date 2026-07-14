using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;
using Xunit;

namespace Transitio.Mediator.Tests;

public class PipelineBehaviorTests
{
    [Fact]
    public async Task Behaviors_run_in_registration_order_around_handler()
    {
        var trace = new PipelineTrace();
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddTransient<IRequestHandler<Ping, string>, PingHandler>();
        services.AddTransient<IPipelineBehavior<Ping, string>>(_ => new TraceBehavior(trace, "A"));
        services.AddTransient<IPipelineBehavior<Ping, string>>(_ => new TraceBehavior(trace, "B"));
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping("hi"));

        Assert.Equal("Pong: hi", result);
        // A is registered first, so it wraps B: A:before, B:before, (handler), B:after, A:after.
        Assert.Equal(new[] { "A:before", "B:before", "B:after", "A:after" }, trace.Entries);
    }

    [Fact]
    public async Task Behavior_can_short_circuit_the_handler()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddTransient<IRequestHandler<Ping, string>, PingHandler>();
        services.AddTransient<IPipelineBehavior<Ping, string>, ShortCircuitBehavior>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping("hi"));

        Assert.Equal("short-circuited", result);
    }

    [Fact]
    public async Task Behavior_can_transform_the_handler_response()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddTransient<IRequestHandler<Ping, string>, PingHandler>();
        services.AddTransient<IPipelineBehavior<Ping, string>, UppercaseBehavior>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping("hi"));

        Assert.Equal("PONG: HI", result);
    }

    [Fact]
    public async Task Behavior_wraps_a_void_request()
    {
        DoWorkHandler.LastTag = null;
        var trace = new PipelineTrace();
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddSingleton(trace);
        services.AddTransient<IRequestHandler<DoWork, Unit>, DoWorkHandler>();
        services.AddTransient<IPipelineBehavior<DoWork, Unit>, VoidTraceBehavior>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await mediator.Send(new DoWork("void-work"));

        Assert.Equal("void-work", DoWorkHandler.LastTag);
        Assert.Equal(new[] { "void:before", "void:after" }, trace.Entries);
    }
}
