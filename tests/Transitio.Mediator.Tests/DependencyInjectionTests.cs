using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;
using Xunit;

namespace Transitio.Mediator.Tests;

// A request with no pipeline behaviors, used to exercise the scanned registrations without picking
// up the Ping behaviors defined elsewhere in this test assembly.
public sealed class Echo : IRequest<string>
{
    public Echo(string text) => Text = text;
    public string Text { get; }
}

public sealed class EchoHandler : IRequestHandler<Echo, string>
{
    public Task<string> Handle(Echo request, CancellationToken cancellationToken)
        => Task.FromResult(request.Text);
}

public class DependencyInjectionTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(EchoHandler).Assembly;

    [Fact]
    public void AddTransitioMediator_registers_mediator_roles()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IMediator>());
        Assert.NotNull(provider.GetService<ISender>());
        Assert.NotNull(provider.GetService<IPublisher>());
    }

    [Fact]
    public async Task Scanned_request_handler_is_discovered_and_invoked()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.Send(new Echo("round-trip"));

        Assert.Equal("round-trip", result);
    }

    [Fact]
    public async Task Scanned_notification_handlers_all_fire()
    {
        var log = new NotificationLog();
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        services.AddSingleton(log); // handlers depend on the shared log
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await mediator.Publish(new Pinged("scan"));

        Assert.Contains("first:scan", log.Entries);
        Assert.Contains("second:scan", log.Entries);
    }

    [Fact]
    public async Task Calling_twice_does_not_double_register_handlers()
    {
        var log = new NotificationLog();
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);
        services.AddTransitioMediator(TestAssembly); // idempotent: must not stack registrations
        services.AddSingleton(log);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await mediator.Publish(new Pinged("once"));

        // Each handler fires exactly once despite the duplicate scan.
        Assert.Equal(2, log.Entries.Count);
    }

    [Fact]
    public void Default_lifetime_is_transient()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(TestAssembly);

        var descriptor = services.Single(d => d.ServiceType == typeof(IRequestHandler<Echo, string>));
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void Custom_lifetime_is_respected()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator(ServiceLifetime.Singleton, TestAssembly);

        var descriptor = services.Single(d => d.ServiceType == typeof(IRequestHandler<Echo, string>));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
