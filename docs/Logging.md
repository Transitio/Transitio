# Logging
 
[← Back to README](../README.md) · [Getting Started](getting-started.md)
 
`Transitio.Logging` is a thin wrapper over `Microsoft.Extensions.Logging` that adds structured,
correlation-aware context — correlation ids that flow across async boundaries, scoped property
enrichment, and operation timing — with a small extension-method API. It is **not** a new logging
backend: it has no Console/File sinks of its own, and you plug in whatever `ILoggerFactory`/
providers you already use (console, Serilog, Application Insights, ...). It is **standalone** — it
does not depend on `Transitio.Mapper`, `Transitio.Validation`, `Transitio.Mediator`, or
`Transitio.Dependency`, only on `Microsoft.Extensions.Logging.Abstractions` (and
`Microsoft.Extensions.DependencyInjection.Abstractions` for `AddTransitioLogging`).
 
```bash
dotnet add package Transitio.Logging
```
 
## Correlation ids
 
`CorrelationContext` flows a correlation id across an async call chain via `AsyncLocal`, without
any DI container in scope.
 
```csharp
using Transitio.Logging;
 
// Mint a fresh id (Guid.NewGuid().ToString("N") by default):
using (CorrelationContext.BeginScope())
{
    Console.WriteLine(CorrelationContext.Current);
}
 
// Nested scopes reuse the ambient id, so a whole logical operation shares one id:
using (CorrelationContext.BeginScope("outer"))
{
    using (CorrelationContext.BeginScope()) // no explicit id -> reuses "outer"
    {
        Console.WriteLine(CorrelationContext.Current); // "outer"
    }
}
 
// An explicit id (e.g. from an inbound X-Correlation-Id header) overrides it:
using (CorrelationContext.BeginScope(incomingHeaderValue))
{
    // ...
}
```
 
Sibling concurrent flows never see each other's id — each `Task.Run`/parallel branch gets its own
ambient value, and disposing a scope restores whatever was ambient before it (stack-like nesting).
 
## Correlated logger scopes
 
`logger.BeginCorrelationScope()` combines `CorrelationContext.BeginScope` with an `ILogger` scope
carrying `CorrelationId` as a structured property, so every log entry written while it's active can
be tied back to the same logical operation:
 
```csharp
using (logger.BeginCorrelationScope())
{
    logger.LogInformation("Processing order {OrderId}", orderId);
}
```
 
## Structured property scopes
 
`logger.BeginScope(("Key", value), ...)` enriches every log entry written for the scope's
duration without repeating the properties on every call:
 
```csharp
using (logger.BeginScope(("OrderId", orderId), ("CustomerId", customerId)))
{
    logger.LogInformation("Validating order");
    logger.LogInformation("Order validated");
}
```
 
> Pass **two or more** properties. A call with exactly one tuple, e.g.
> `logger.BeginScope(("Key", value))`, binds to `ILogger`'s own `BeginScope<TState>(TState state)`
> instance method instead of this extension — C# always prefers an applicable instance method over
> an extension method. The scope still works, but loses the "Key=Value" `ToString()` rendering.
> Use `BeginCorrelationScope` for the single correlation-id case instead.
 
## Timing operations
 
`logger.TimeOperation("Name")` logs elapsed milliseconds on dispose, using a structured message
template so providers capture `OperationName`/`ElapsedMilliseconds` as separate fields:
 
```csharp
using (logger.TimeOperation("PlaceOrder"))
{
    await orderService.PlaceOrderAsync(request);
}
// logs: "PlaceOrder completed in 42ms"
 
using (logger.TimeOperation("CacheWarmup", LogLevel.Debug)) // optional level override
{
    await cache.WarmUpAsync();
}
```
 
## Configuration
 
`AddTransitioLogging` accepts an optional configuration delegate. Currently the only option is
`CorrelationIdGenerator`, an override for how a fresh id is minted:
 
```csharp
services.AddTransitioLogging(options =>
    options.CorrelationIdGenerator = () => $"req-{Guid.NewGuid():N}");
```
 
## Dependency injection
 
```csharp
services.AddLogging(builder => builder.AddConsole()); // your own ILoggerFactory setup
services.AddTransitioLogging();
```
 
`AddTransitioLogging(...)` registers `TransitioLoggingOptions` as a singleton and applies any
static configuration (currently just `CorrelationIdGenerator`, copied onto the static
`CorrelationContext.IdGenerator`). It does **not** register an `ILoggerFactory` or any provider —
call `services.AddLogging(...)` (or your host's own logging setup) separately; Transitio.Logging
only adds context on top of whatever `ILogger` you already have.
 
## Integrating with other Transitio packages
 
Transitio.Logging never references Transitio.Mapper, Transitio.Validation, or Transitio.Mediator —
that would break the "standalone, install what you need" model every package here follows.
Instead, here are copy-paste recipes for wiring it into each. A full, runnable version of the
Mediator recipe lives in `samples/BasicSample/LoggingFeaturesDemo.cs`, and an end-to-end proof that
correlation ids survive the full Mediator pipeline lives in `tests/Transitio.Integration.Tests`.
 
### Mediator: a correlation-aware `LoggingBehavior<TRequest, TResponse>`
 
Open-generic pipeline behaviors are discovered automatically by `AddTransitioMediator`'s assembly
scan — no manual registration needed:
 
```csharp
public sealed class CorrelationLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CorrelationLoggingBehavior<TRequest, TResponse>> _logger;
 
    public CorrelationLoggingBehavior(ILogger<CorrelationLoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;
 
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var correlationScope = _logger.BeginCorrelationScope();
        using var timer = _logger.TimeOperation(typeof(TRequest).Name);
 
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}
```
 
> **This applies to every request type `AddTransitioMediator` discovers in the scanned assembly,
> unconditionally.** An open-generic `IPipelineBehavior<,>` has no way to opt individual request
> types out, and if logging isn't configured in that container, the failure surfaces as a
> confusing `ILogger<T>` resolution error at the first `Send` call — not anything that points at
> behavior registration. Make sure `services.AddLogging(...)` runs before `AddTransitioMediator`
> in any container that scans an assembly containing this behavior.
 
### Validation: a logging decorator for `IValidator<T>`
 
This repo has no Scrutor-style automatic decoration, so decoration is done by re-registering the
`ServiceDescriptor` by hand, after `AddTransitioValidation` has already registered the real
validators:
 
```csharp
public sealed class LoggingValidatorDecorator<T> : IValidator<T>
{
    private readonly IValidator<T> _inner;
    private readonly ILogger<LoggingValidatorDecorator<T>> _logger;
 
    public LoggingValidatorDecorator(IValidator<T> inner, ILogger<LoggingValidatorDecorator<T>> logger)
    {
        _inner = inner;
        _logger = logger;
    }
 
    public ValidationResult Validate(T instance)
    {
        var result = _inner.Validate(instance);
        if (!result.IsValid)
            _logger.LogWarning("Validation failed for {Type}: {Errors}", typeof(T).Name, result);
        return result;
    }
 
    ValidationResult IValidator.Validate(object instance) => Validate((T)instance);
}
 
public static IServiceCollection DecorateValidatorWithLogging<T>(this IServiceCollection services)
{
    var original = services.Single(d => d.ServiceType == typeof(IValidator<T>));
    services.Remove(original);
 
    services.Add(ServiceDescriptor.Describe(
        typeof(IValidator<T>),
        sp =>
        {
            var inner = (IValidator<T>)(original.ImplementationFactory?.Invoke(sp)
                ?? ActivatorUtilities.CreateInstance(sp, original.ImplementationType!));
            return new LoggingValidatorDecorator<T>(inner, sp.GetRequiredService<ILogger<LoggingValidatorDecorator<T>>>());
        },
        original.Lifetime));
 
    return services;
}
 
// services.AddTransitioValidation(typeof(CustomerValidator).Assembly);
// services.DecorateValidatorWithLogging<Customer>();
```
 
### Mapper: a logging decorator for `IMapper`
 
```csharp
public sealed class LoggingMapperDecorator : IMapper
{
    private readonly IMapper _inner;
    private readonly ILogger<LoggingMapperDecorator> _logger;
 
    public LoggingMapperDecorator(IMapper inner, ILogger<LoggingMapperDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }
 
    public TDestination Map<TDestination>(object source)
    {
        try
        {
            return _inner.Map<TDestination>(source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Map to {Destination} failed", typeof(TDestination).Name);
            throw;
        }
    }
}
 
public static IServiceCollection DecorateMapperWithLogging(this IServiceCollection services)
{
    var original = services.Single(d => d.ServiceType == typeof(IMapper));
    services.Remove(original);
 
    services.Add(ServiceDescriptor.Describe(
        typeof(IMapper),
        sp =>
        {
            var inner = (IMapper)(original.ImplementationFactory?.Invoke(sp)
                ?? ActivatorUtilities.CreateInstance(sp, original.ImplementationType!));
            return new LoggingMapperDecorator(inner, sp.GetRequiredService<ILogger<LoggingMapperDecorator>>());
        },
        original.Lifetime));
 
    return services;
}
 
// services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
// services.DecorateMapperWithLogging();
```
 
## Not in this version
 
The following are planned for a later release: log-level filtering/enrichment configuration
beyond `CorrelationIdGenerator`, built-in enrichers (machine name, environment, thread id), and
distributed-tracing (`Activity`/W3C `traceparent`) correlation.