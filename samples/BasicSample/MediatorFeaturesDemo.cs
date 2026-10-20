using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

// A void request (no response) - demonstrates IRequest/IRequestHandler<TRequest> (Unit) instead
// of the value-returning IRequest<TResponse>/IRequestHandler<TRequest,TResponse> above.
public sealed class DoWork : IRequest
{
    public DoWork(string tag) => Tag = tag;
    public string Tag { get; }
}

public sealed class DoWorkHandler : IRequestHandler<DoWork>
{
    public Task Handle(DoWork request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [work] completed '{request.Tag}'");
        return Task.CompletedTask;
    }
}

// A CLOSED pipeline behavior - fixed to Greet/string, unlike the open-generic LoggingBehavior<,>
// below. Discovered by the same assembly scan via the closed-interface path, not the open-generic
// path.
public sealed class GreetAuditBehavior : IPipelineBehavior<Greet, string>
{
    public async Task<string> Handle(Greet request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [audit] auditing Greet for {request.Name}");
        return await next();
    }
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

// An open-generic pipeline behavior that wraps every request. AddTransitioMediator's assembly
// scan discovers it automatically and registers it against the open IPipelineBehavior<,> service
// type, so no manual registration is needed.
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

// A notification with a handler that throws (declared first) and one that succeeds (declared
// second) - both run even though the first one fails, and the failure surfaces afterward as an
// AggregateException.
public sealed class FlakyNotification : INotification
{
    public FlakyNotification(string tag) => Tag = tag;
    public string Tag { get; }
}

public sealed class ThrowingHandler : INotificationHandler<FlakyNotification>
{
    public Task Handle(FlakyNotification notification, CancellationToken cancellationToken)
        => throw new InvalidOperationException($"boom handling '{notification.Tag}'");
}

public sealed class SucceedingHandler : INotificationHandler<FlakyNotification>
{
    public Task Handle(FlakyNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [flaky] succeeding handler still ran for '{notification.Tag}'");
        return Task.CompletedTask;
    }
}

// A notification with zero registered handlers - Publish should be a no-op, not an error.
public sealed class UnhandledNotification : INotification
{
}

// A behavior fixed to a specific response type (string) - generic only in TRequest. Only
// behaviors generic in BOTH type parameters are auto-discovered by AddTransitioMediator's
// assembly scan, so this one is registered manually instead (see RunAsync step 8 for why that
// registration has to close TRequest explicitly, unlike a fully-open behavior).
public sealed class StringResponseAuditBehavior<TRequest> : IPipelineBehavior<TRequest, string>
    where TRequest : IRequest<string>
{
    public async Task<string> Handle(
        TRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [manual-partial-generic] wrapping {typeof(TRequest).Name}");
        return await next();
    }
}

public static class MediatorFeaturesDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Transitio.Mediator features ===");

        // Scanning this assembly discovers handlers and every open-generic IPipelineBehavior<,> in
        // it, including LoggingFeatures.CorrelationLoggingBehavior<,> (which needs ILogger<>) - so
        // AddLogging() is required here even though this demo doesn't log anything itself.
        var provider = new ServiceCollection()
            .AddLogging()
            .AddTransitioMediator(typeof(MediatorFeaturesDemo).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // 1. Send a request through the pipeline to its single handler. GreetAuditBehavior
        //    (closed) and LoggingBehavior<,> (open-generic) both wrap it.
        var greeting = await mediator.Send(new Greet("Ada"));
        Console.WriteLine($"Send(Greet) -> {greeting}");

        // 2. Publish a notification to every registered handler.
        Console.WriteLine("Publish(UserRegistered) ->");
        await mediator.Publish(new UserRegistered("ada@example.com"));

        // 3. A void request (IRequest / IRequestHandler<TRequest>) - no response value.
        await mediator.Send(new DoWork("nightly-cleanup"));

        // 4. AddTransitioMediator with an explicit ServiceLifetime, overriding the default
        //    Transient, in its own container.
        var singletonProvider = new ServiceCollection()
            .AddLogging()
            .AddTransitioMediator(ServiceLifetime.Singleton, typeof(MediatorFeaturesDemo).Assembly)
            .BuildServiceProvider();

        var singletonMediator = singletonProvider.GetRequiredService<IMediator>();
        Console.WriteLine($"Send(Greet) with Singleton lifetime -> {await singletonMediator.Send(new Greet("Grace"))}");

        // 5. A notification with a handler that throws: the other handler for the same
        //    notification still runs, and the failure surfaces afterward as an AggregateException
        //    - one faulty handler never silently swallows the rest.
        try
        {
            await mediator.Publish(new FlakyNotification("batch-1"));
            Console.WriteLine("- (unexpected) FlakyNotification completed without throwing");
        }
        catch (AggregateException ex)
        {
            Console.WriteLine($"- Publish surfaced {ex.InnerExceptions.Count} failure(s) as AggregateException: {ex.InnerExceptions[0].Message}");
        }

        // 6. Publishing a notification with zero registered handlers is a no-op.
        await mediator.Publish(new UnhandledNotification());
        Console.WriteLine("Publish(UnhandledNotification) with no handlers -> completed as a no-op");

        // 7. Response-widening footgun: IRequest<out TResponse> is covariant, so Send<object>(...)
        //    compiles, but no handler is registered against IRequestHandler<Greet, object> (only
        //    IRequestHandler<Greet, string> is) - it throws instead of silently succeeding. Uses a
        //    fresh provider/mediator: the mediator caches one wrapper per request type (not per
        //    request+response pair), so reusing the `mediator` above - which already built and
        //    cached a Greet/string wrapper earlier in this demo - would hit that cached wrapper and
        //    throw an unrelated InvalidCastException instead of the documented "no handler" error.
        var widenedServices = new ServiceCollection().AddLogging();
        widenedServices.AddTransitioMediator(typeof(MediatorFeaturesDemo).Assembly);
        var widenedMediator = widenedServices.BuildServiceProvider().GetRequiredService<IMediator>();
        try
        {
            await widenedMediator.Send<object>(new Greet("Widened"));
            Console.WriteLine("- (unexpected) widened Send succeeded");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"- Send<object>(Greet) found no handler: {ex.Message}");
        }

        // 8. A partially-generic behavior (fixed to TResponse=string) isn't auto-discovered by the
        //    assembly scan - only behaviors generic in both type parameters are. Unlike a
        //    fully-open behavior (both type params free, same arity as IPipelineBehavior<,>), the
        //    stock DI container refuses services.AddTransient(typeof(IPipelineBehavior<,>),
        //    typeof(StringResponseAuditBehavior<>)) outright - "Arity of open generic service type
        //    ... does not equal arity of open generic implementation type" - since the service has
        //    2 open parameters and the behavior only has 1 (TResponse is fixed to string). Manual
        //    registration for a partially-generic behavior has to close it per request type instead.
        var manualServices = new ServiceCollection().AddLogging();
        manualServices.AddTransitioMediator(typeof(MediatorFeaturesDemo).Assembly);
        manualServices.AddTransient<IPipelineBehavior<Greet, string>, StringResponseAuditBehavior<Greet>>();
        var manualProvider = manualServices.BuildServiceProvider();
        var manualMediator = manualProvider.GetRequiredService<IMediator>();
        Console.WriteLine($"Send(Greet) with manually-registered partial-generic behavior -> {await manualMediator.Send(new Greet("Manual"))}");
    }
}