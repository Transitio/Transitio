using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;
using Xunit;

namespace Transitio.Mediator.Tests;

public class MediatorSendTests
{
    private static IMediator BuildMediator(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(); // registers the mediator; handlers added below
        configure(services);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_request_returns_handler_response()
    {
        var mediator = BuildMediator(s =>
            s.AddTransient<IRequestHandler<Ping, string>, PingHandler>());

        var result = await mediator.Send(new Ping("hi"));

        Assert.Equal("Pong: hi", result);
    }

    [Fact]
    public async Task Send_void_request_runs_handler()
    {
        DoWorkHandler.LastTag = null;
        // A void handler implements both IRequestHandler<DoWork> and IRequestHandler<DoWork, Unit>;
        // the dispatch pipeline resolves the Unit-returning one (what assembly scanning registers).
        var mediator = BuildMediator(s =>
            s.AddTransient<IRequestHandler<DoWork, Unit>, DoWorkHandler>());

        await mediator.Send(new DoWork("work-42"));

        Assert.Equal("work-42", DoWorkHandler.LastTag);
    }

    [Fact]
    public async Task Send_unregistered_request_throws_clear_error()
    {
        var mediator = BuildMediator(_ => { });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new Ping("hi")));

        Assert.Contains("No handler registered", ex.Message);
    }

    [Fact]
    public async Task Send_null_request_throws()
    {
        var mediator = BuildMediator(_ => { });

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => mediator.Send<string>(null!));
    }
}
