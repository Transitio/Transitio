# Mediator

[← Back to README](../README.md) · [Getting Started](getting-started.md)

`Transitio.Mediator` is a lightweight in-process mediator (CQRS-style). Send a request to its
single handler, or publish a notification to many handlers, with optional pipeline behaviors for
cross-cutting concerns. It is **standalone** — it does not depend on `Transitio.Mapper` or
`Transitio.Dependency`, only on `Microsoft.Extensions.DependencyInjection`.

```bash
dotnet add package Transitio.Mediator
```

## Requests and handlers

A request implements `IRequest<TResponse>`; exactly one `IRequestHandler<TRequest, TResponse>`
handles it.

```csharp
using Transitio.Mediator;

public record Ping(string Message) : IRequest<string>;

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}

string reply = await mediator.Send(new Ping("hi")); // "Pong: hi"
```

Let the response type be inferred from the request (`Send(new Ping(...))`). Because
`IRequest<out TResponse>` is covariant, explicitly widening the response — e.g.
`Send<object>(new Ping(...))` — resolves no handler at runtime, since handlers are registered
against the request's declared response type (`string` here).

### Void requests

For requests that produce no value, implement `IRequest` and `IRequestHandler<TRequest>` — the
response is `Unit` behind the scenes, so the same pipeline serves both:

```csharp
public record DoWork(string Tag) : IRequest;

public class DoWorkHandler : IRequestHandler<DoWork>
{
    public Task Handle(DoWork request, CancellationToken cancellationToken)
    {
        // ... do the work ...
        return Task.CompletedTask;
    }
}

await mediator.Send(new DoWork("nightly"));
```

## Notifications

A notification implements `INotification` and is delivered to every registered
`INotificationHandler<TNotification>`. Handlers run **sequentially** in registration order; zero
handlers is a no-op. Every handler is invoked even if an earlier one throws — the collected
failures are surfaced together as an `AggregateException`, so one faulty handler never silently
skips the rest.

```csharp
public record UserRegistered(string Email) : INotification;

public class SendWelcomeEmail : INotificationHandler<UserRegistered>
{
    public Task Handle(UserRegistered notification, CancellationToken cancellationToken) { /* ... */ }
}

public class WriteAuditLog : INotificationHandler<UserRegistered>
{
    public Task Handle(UserRegistered notification, CancellationToken cancellationToken) { /* ... */ }
}

await mediator.Publish(new UserRegistered("ada@example.com")); // both handlers fire
```

## Pipeline behaviors

An `IPipelineBehavior<TRequest, TResponse>` wraps request handling to add cross-cutting behavior
(logging, validation, transactions). Behaviors run in registration order around the handler; call
`next()` to continue the pipeline or skip it to short-circuit, and you can transform the response.

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // before
        var response = await next(); // invoke the next behavior / the handler
        // after
        return response;
    }
}
```

## Dependency injection

`AddTransitioMediator` registers the mediator (`IMediator`, `ISender`, `IPublisher`) and scans
assemblies for concrete request handlers, notification handlers, and **closed** pipeline behaviors,
registering each as its closed interface. Handlers are created by the container, so they may take
constructor dependencies.

```csharp
using Transitio.Mediator;

services.AddTransitioMediator(typeof(PingHandler).Assembly);

// Resolve and use:
var mediator = provider.GetRequiredService<IMediator>();
```

The default lifetime is `Transient`; pass a `ServiceLifetime` to override it.

**Open-generic** pipeline behaviors (e.g. `LoggingBehavior<,>`) are not discovered by the
closed-interface scan and must be registered manually:

```csharp
services.AddTransitioMediator(typeof(PingHandler).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## Not in this version

The following are planned for a later release: streaming requests (`IStreamRequest` /
`IAsyncEnumerable`), configurable notification publishing strategies (parallel / whenAll), and
`RequestPreProcessor` / `RequestPostProcessor` hooks.
