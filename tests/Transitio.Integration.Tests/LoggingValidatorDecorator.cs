using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Transitio.Dependency;
using Transitio.Mapper;
using Transitio.Validation;
using Xunit;
 
namespace Transitio.Integration.Tests;
 
// Verifies the two manual-decoration recipes published in docs/logging.md (Validation and
// Mapper) actually compile and work against the real APIs — those recipes were hand-written
// documentation, never previously compiled, so this guards against doc/code drift.
 
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
 
public static class LoggingDecoratorRecipeExtensions
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
 
public class LoggingDecoratorRecipeTests
{
    [Fact]
    public void DecoratedValidator_Still_Validates_Correctly()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddTransitioValidation(typeof(PlaceOrderValidator).Assembly);
        services.DecorateValidatorWithLogging<PlaceOrderRequest>();
 
        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IValidator<PlaceOrderRequest>>();
 
        Assert.IsType<LoggingValidatorDecorator<PlaceOrderRequest>>(validator);
 
        var valid = new PlaceOrderRequest { CustomerName = "Ada", Email = "ada@example.com", Amount = 10m };
        var invalid = new PlaceOrderRequest { CustomerName = "", Email = "bad", Amount = -1m };
 
        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(invalid).IsValid);
    }
 
    [Fact]
    public void DecoratedMapper_Still_Maps_Correctly()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddTransitio(cfg => cfg.CreateMap<PlaceOrderRequest, OrderResult>());
        services.DecorateMapperWithLogging();
 
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
 
        Assert.IsType<LoggingMapperDecorator>(mapper);
 
        var result = mapper.Map<OrderResult>(new PlaceOrderRequest { CustomerName = "Grace", Amount = 5m });
        Assert.Equal("Grace", result.CustomerName);
        Assert.Equal(5m, result.Amount);
    }
 
    [Fact]
    public void DecoratedMapper_Rethrows_And_Logs_On_Failure()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddTransitio(cfg => { }); // no CreateMap registered for PlaceOrderRequest -> OrderResult
        services.DecorateMapperWithLogging();
 
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
 
        Assert.Throws<InvalidOperationException>(() => mapper.Map<OrderResult>(new PlaceOrderRequest()));
    }
}