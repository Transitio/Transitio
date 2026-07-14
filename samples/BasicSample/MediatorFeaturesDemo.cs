using Microsoft.Extensions.DependencyInjection;
using Transitio.Mediator;

namespace BasicSample.MediatorFeatures;

// ============================================================================
// Requests, notifications, handlers, and a pipeline behavior used by the
// Transitio.Mediator feature demo.
// ============================================================================

// A request that returns a value.
public sealed class Greet : IRequest<string>
{
    public Greet(string name) => Name = name;
    public string Name { get; }
}

public sealed class GreetHandler : IRequestHandler<Greet, string>
{
    public Task<string> Handle(Greet request, CancellationToken cancellationToken)
        => Task.FromResult($"Hello, {request.Name}!");
}

// A notification with two handlers; both fire on publish.
public sealed class UserRegistered : INotification
{
    public UserRegistered(string email) => Email = email;
    public string Email { get; }
}

public sealed class SendWelcomeEmail : INotificationHandler<UserRegistered>
{
    public Task Handle(UserRegistered notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [email]  welcome sent to {notification.Email}");
        return Task.CompletedTask;
    }
}

public sealed class WriteAuditLog : INotificationHandler<UserRegistered>
{
    public Task Handle(UserRegistered notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [audit]  registered {notification.Email}");
        return Task.CompletedTask;
    }
}

// An open-generic pipeline behavior that wraps every request. Open generics are not discovered by
// assembly scanning, so it is registered manually below.
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [pipeline] before {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"  [pipeline] after  {typeof(TRequest).Name}");
        return response;
    }
}

public static class MediatorFeaturesDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Transitio.Mediator features ===");

        // Scan this assembly for handlers, then register the open-generic behavior manually.
        var provider = new ServiceCollection()
            .AddTransitioMediator(typeof(MediatorFeaturesDemo).Assembly)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // 1. Send a request through the pipeline to its single handler.
        var greeting = await mediator.Send(new Greet("Ada"));
        Console.WriteLine($"Send(Greet) -> {greeting}");

        // 2. Publish a notification to every registered handler.
        Console.WriteLine("Publish(UserRegistered) ->");
        await mediator.Publish(new UserRegistered("ada@example.com"));
    }
}
