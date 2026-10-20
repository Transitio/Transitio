using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Logging;
using Transitio.Mapper;
using Transitio.Mediator;
using Transitio.Validation;
using Xunit;
 
namespace Transitio.Integration.Tests;
 
// Records the CorrelationContext.Current value observed inside the Mediator pipeline, proving a
// correlation id set by the caller's BeginCorrelationScope flows, via AsyncLocal, through the full
// Mediator pipeline (validation behavior, handler, mapper) and back out. The observations
// dependency is optional (defaults to null) so this open-generic behavior - discovered by every
// AddTransitioMediator(assembly) scan in this project, including the OTHER integration tests'
// containers that never register CorrelationObservations - is a harmless no-op there instead of
// a DI resolution failure.
public sealed class CorrelationRecordingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly CorrelationObservations? _observations;
 
    public CorrelationRecordingBehavior(CorrelationObservations? observations = null)
        => _observations = observations;
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_observations != null)
            _observations.SeenInBehavior = CorrelationContext.Current;
 
        return await next();
    }
}
 
public sealed class CorrelationObservations
{
    public string? SeenInBehavior;
}
 
public class CorrelationLoggingPipelineTests
{
    [Fact]
    public async Task CorrelationId_FlowsThrough_ValidationAndHandler_ViaAsyncLocal()
    {
        var observations = new CorrelationObservations();
 
        var services = new ServiceCollection();
        services.AddSingleton(observations);
        services.AddSingleton<OrderProcessingTracker>();
        services.AddTransitio(cfg => cfg.CreateMap<PlaceOrderRequest, OrderResult>());
        services.AddTransitioValidation(typeof(PlaceOrderValidator).Assembly);
        services.AddTransitioMediator(typeof(PlaceOrderHandler).Assembly); // also discovers CorrelationRecordingBehavior<,> above
 
        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
 
        using (CorrelationContext.BeginScope("test-correlation-id"))
        {
            await mediator.Send(new PlaceOrderRequest
            {
                CustomerName = "Ada",
                Email = "ada@example.com",
                Amount = 10m,
            });
        }
 
        Assert.Equal("test-correlation-id", observations.SeenInBehavior);
    }
}