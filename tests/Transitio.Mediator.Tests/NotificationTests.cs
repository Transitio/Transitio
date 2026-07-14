using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;
using Xunit;

namespace Transitio.Mediator.Tests;

public class NotificationTests
{
    [Fact]
    public async Task Publish_invokes_all_registered_handlers()
    {
        var log = new NotificationLog();
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddSingleton(log);
        services.AddTransient<INotificationHandler<Pinged>, FirstPingedHandler>();
        services.AddTransient<INotificationHandler<Pinged>, SecondPingedHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await mediator.Publish(new Pinged("src"));

        Assert.Contains("first:src", log.Entries);
        Assert.Contains("second:src", log.Entries);
        Assert.Equal(2, log.Entries.Count);
    }

    [Fact]
    public async Task Publish_with_no_handlers_is_noop()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Should not throw.
        await mediator.Publish(new Unheard());
    }

    [Fact]
    public async Task Publish_runs_every_handler_even_when_one_throws()
    {
        var log = new NotificationLog();
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        services.AddSingleton(log);
        services.AddTransient<INotificationHandler<Buzzed>, ThrowingBuzzedHandler>();
        services.AddTransient<INotificationHandler<Buzzed>, RecordingBuzzedHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var ex = await Assert.ThrowsAsync<AggregateException>(
            () => mediator.Publish(new Buzzed()));

        // The failing handler is surfaced, but the recording handler registered after it still ran.
        Assert.Single(ex.InnerExceptions);
        Assert.Contains("buzzed", log.Entries);
    }

    [Fact]
    public async Task Publish_object_overload_rejects_non_notification()
    {
        var services = new ServiceCollection();
        services.AddTransitioMediator();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<System.ArgumentException>(
            () => mediator.Publish((object)"not a notification"));
    }
}
