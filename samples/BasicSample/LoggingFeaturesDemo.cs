using System.Linq;
using BasicSample.MediatorFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transitio.Dependency;
using Transitio.Logging;
using Transitio.Mapper;
using Transitio.Mediator;
using Transitio.Validation;
 
namespace BasicSample.LoggingFeatures;
 
// A type/validator pair private to this demo so AddTransitioValidation's assembly scan (which
// covers the whole BasicSample assembly) finds exactly one IValidator<> for it - Customer, used
// elsewhere in this sample, already has two (CustomerValidator and StrictCustomerValidator),
// which would make DecorateValidatorWithLogging<T>'s .Single(...) throw.
public sealed class LoggingDemoOrder
{
    public string CustomerName { get; set; } = "";
}
 
public sealed class LoggingDemoOrderValidator : AbstractValidator<LoggingDemoOrder>
{
    public LoggingDemoOrderValidator() => RuleFor(o => o.CustomerName).NotEmpty();
}
 
// The Mediator recipe from docs/logging.md, using Transitio.Logging's correlation scope and
// operation timer instead of raw Console.WriteLine. Distinct from MediatorFeatures.LoggingBehavior<,>
// (the Console-only demo behavior) so there's no ambiguity about which recipe this demonstrates.
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
 
        _logger.LogInformation("Handling {RequestType} (CorrelationId={CorrelationId})",
            typeof(TRequest).Name, CorrelationContext.Current);
        return await next();
    }
}
 
// The Validation recipe from docs/logging.md: a manual decorator around IValidator<T>, since this
// repo has no Scrutor-style automatic decoration.
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
 
// The Mapper recipe from docs/logging.md: a manual decorator around IMapper, logging (and
// rethrowing) on failure.
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
 
public static class LoggingDecoratorExtensions
{
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
}
 
public static class LoggingFeaturesDemo
{
    // Wraps a real ILogger and prints the runtime type of whatever state object BeginScope<TState>
    // actually received - used below to make the single-tuple overload-resolution caveat visible
    // rather than just documented.
    private sealed class ScopeTypeProbeLogger : ILogger
    {
        private readonly ILogger _inner;
        public ScopeTypeProbeLogger(ILogger inner) => _inner = inner;
 
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            Console.WriteLine($"  -> BeginScope<TState> received a {state.GetType().Name}");
            return _inner.BeginScope(state);
        }
 
        public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);
 
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => _inner.Log(logLevel, eventId, state, exception, formatter);
    }
 
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Transitio.Logging features ===");
 
        var provider = new ServiceCollection()
            .AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole())
            .AddTransitioLogging(options =>
                options.CorrelationIdGenerator = () => "demo-" + Guid.NewGuid().ToString("N")[..8])
            .AddTransitioMediator(typeof(LoggingFeaturesDemo).Assembly)
            .BuildServiceProvider();
 
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("LoggingFeaturesDemo");
 
        // 0. Raw CorrelationContext.BeginScope(): mint-fresh, nested-reuse, explicit-override,
        //    and restore-on-dispose - without going through a logger scope at all.
        Console.WriteLine($"Before any scope, Current is null: {CorrelationContext.Current is null}");
        using (CorrelationContext.BeginScope()) // mints a fresh id
        {
            Console.WriteLine($"Outer scope minted: {CorrelationContext.Current}");
 
            using (CorrelationContext.BeginScope()) // no explicit id -> reuses the outer one
            {
                Console.WriteLine($"Inner scope (no explicit id) reused: {CorrelationContext.Current}");
            }
 
            using (CorrelationContext.BeginScope("explicit-override")) // explicit id overrides it
            {
                Console.WriteLine($"Inner scope (explicit id) overrode to: {CorrelationContext.Current}");
            }
 
            Console.WriteLine($"Back in outer scope, still: {CorrelationContext.Current}");
        }
        Console.WriteLine($"After the outer scope disposed, Current is null again: {CorrelationContext.Current is null}");
 
        // 0b. Concurrency isolation: CorrelationContext is backed by AsyncLocal<string?>, which
        //     flows down into awaited continuations but never across sibling branches - two
        //     parallel Task.Run flows each mint their own id and never see the other's.
        var flowA = Task.Run(async () =>
        {
            using (CorrelationContext.BeginScope())
            {
                var mintedId = CorrelationContext.Current;
                await Task.Delay(30);
                return (MintedId: mintedId, StillSame: CorrelationContext.Current == mintedId);
            }
        });
        var flowB = Task.Run(async () =>
        {
            using (CorrelationContext.BeginScope())
            {
                var mintedId = CorrelationContext.Current;
                await Task.Delay(10);
                return (MintedId: mintedId, StillSame: CorrelationContext.Current == mintedId);
            }
        });
        var (resultA, resultB) = (await flowA, await flowB);
        Console.WriteLine($"Sibling Task.Run flows minted different ids: {resultA.MintedId != resultB.MintedId}");
        Console.WriteLine($"Each flow's id was stable across its own await: {resultA.StillSame && resultB.StillSame}");
 
        // 1. Standalone: BeginCorrelationScope + BeginScope(properties) + TimeOperation
        //    (including the optional LogLevel override).
        using (logger.BeginCorrelationScope())
        using (logger.BeginScope(("Feature", "LoggingDemo"), ("Step", 1)))
        {
            using (logger.TimeOperation("StandaloneWork"))
                await Task.Delay(10);
 
            using (logger.TimeOperation("DebugLevelWork", LogLevel.Debug))
                await Task.Delay(5);
 
            logger.LogInformation("Correlation id inside scope: {CorrelationId}", CorrelationContext.Current);
        }
 
        // 1b. The single-tuple BeginScope overload-resolution caveat: ILogger.BeginScope<TState> is
        //     an instance method, and C# only falls back to an extension method when no instance
        //     method is applicable. A single-tuple argument matches BeginScope<TState>(TState)
        //     directly (TState inferred as the tuple type), so it binds there instead of to
        //     LoggerExtensions.BeginScope(params (string,object?)[]) - the state is the raw
        //     ValueTuple, not a LogProperties, so it loses LogProperties' "Key=Value" ToString().
        //     With 2+ tuples the instance method's single-parameter signature no longer applies, so
        //     the params-array extension is the only candidate and LogProperties is used.
        Console.WriteLine("BeginScope overload-resolution caveat:");
        var probeLogger = new ScopeTypeProbeLogger(logger);
        using (probeLogger.BeginScope(("SingleProp", "value"))) // binds to ILogger's own method
        {
        }
        using (probeLogger.BeginScope(("First", "a"), ("Second", "b"))) // binds to the extension
        {
        }
 
        // 2. The same recipe wired into a real Mediator pipeline. CorrelationLoggingBehavior<,>
        //    above is discovered automatically by AddTransitioMediator's open-generic scan, and so
        //    is MediatorFeatures.LoggingBehavior<,> from the same assembly - both wrap Greet below;
        //    that's two independent cross-cutting behaviors composing, not a bug.
        var mediator = provider.GetRequiredService<IMediator>();
        var greeting = await mediator.Send(new Greet("Ada"));
        Console.WriteLine($"Send(Greet) -> {greeting}");
 
        // 3. The Validation decorator recipe: logs a warning when validation fails.
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole());
        services.AddTransitioValidation(typeof(LoggingDemoOrderValidator).Assembly);
        services.DecorateValidatorWithLogging<LoggingDemoOrder>();
        var validationProvider = services.BuildServiceProvider();
 
        var validator = validationProvider.GetRequiredService<IValidator<LoggingDemoOrder>>();
        validator.Validate(new LoggingDemoOrder { CustomerName = "" }); // logs a warning
        Console.WriteLine("Validated a LoggingDemoOrder through the logging decorator (see warning above).");
 
        // 4. The Mapper decorator recipe: logs (and rethrows) when mapping fails.
        var mapperServices = new ServiceCollection();
        mapperServices.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole());
        mapperServices.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
        mapperServices.DecorateMapperWithLogging();
        var mapperProvider = mapperServices.BuildServiceProvider();
 
        var mapper = mapperProvider.GetRequiredService<IMapper>();
        var dto = mapper.Map<UserDto>(new User { Name = "Grace", Age = 36 });
        Console.WriteLine($"Mapped through the logging decorator -> {dto.Name}, {dto.Age}");
 
        // The decorator's defining behavior: an unmapped type throws, and the decorator logs
        // (and rethrows) the failure rather than swallowing it.
        try
        {
            mapper.Map<UserDto>(new LoggingDemoOrder { CustomerName = "unmapped type" });
            Console.WriteLine("- (unexpected) mapping an unregistered type succeeded");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"- caught after the decorator logged it: {ex.Message}");
        }
    }
}