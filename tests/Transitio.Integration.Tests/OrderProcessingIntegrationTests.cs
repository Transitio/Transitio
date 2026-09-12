using Microsoft.Extensions.DependencyInjection;
using Transitio.Assertions;
using Transitio.Dependency;
using Transitio.Mapper;
using Transitio.Mediator;
using Transitio.Validation;
using Xunit;
 
namespace Transitio.Integration.Tests;
 
/// <summary>
/// Composes Transitio.Dependency + Transitio.Mapper + Transitio.Validation + Transitio.Mediator
/// in a single container and drives a request all the way through: an open-generic pipeline
/// behavior (Milestone 1) validates the request via Transitio.Validation before the handler maps
/// it via Transitio.Mapper. Assertions use Transitio.Assertions, so all five packages participate.
/// Unlike the per-package unit test suites, nothing here mocks the other packages out.
/// </summary>
public class OrderProcessingIntegrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
 
        services.AddTransitio(cfg => cfg.CreateMap<PlaceOrderRequest, OrderResult>());
        services.AddTransitioValidation(typeof(PlaceOrderValidator).Assembly);
        services.AddTransitioMediator(typeof(PlaceOrderHandler).Assembly); // discovers the handler AND the open-generic ValidationBehavior<,>
        services.AddSingleton<OrderProcessingTracker>();
 
        return services.BuildServiceProvider();
    }
 
    [Fact]
    public async Task Valid_request_flows_through_validation_mediator_and_mapper()
    {
        using var provider = BuildProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var tracker = provider.GetRequiredService<OrderProcessingTracker>();
 
        var result = await mediator.Send(new PlaceOrderRequest
        {
            CustomerName = "Ada Lovelace",
            Email = "ada@example.com",
            Amount = 100m,
        });
 
        result.Should().NotBeNull();
        result.CustomerName.Should().Be("Ada Lovelace");
        result.Amount.Should().Be(100m);
        result.Status.Should().Be("Placed");
        tracker.ProcessedCount.Should().Be(1);
    }
 
    [Fact]
    public async Task Invalid_request_is_rejected_by_the_validation_behavior_before_the_handler_runs()
    {
        using var provider = BuildProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var tracker = provider.GetRequiredService<OrderProcessingTracker>();
 
        Func<Task> act = () => mediator.Send(new PlaceOrderRequest
        {
            CustomerName = "",
            Email = "not-an-email",
            Amount = -5m,
        });
 
        var thrown = await act.Should().ThrowAsync<ValidationException>();
        thrown.Which.Errors.Count.Should().BeGreaterThan(0);
 
        // The pipeline behavior short-circuited before next() — the handler never ran.
        tracker.ProcessedCount.Should().Be(0);
    }
 
    [Fact]
    public void Validator_and_mapper_are_independently_resolvable_alongside_the_mediator()
    {
        using var provider = BuildProvider();
 
        // Prove the individual packages are also usable directly through the same container,
        // not just through the mediator pipeline.
        var mapper = provider.GetRequiredService<IMapper>();
        var validator = provider.GetRequiredService<IValidator<PlaceOrderRequest>>();
 
        var request = new PlaceOrderRequest { CustomerName = "Grace Hopper", Email = "grace@example.com", Amount = 42m };
 
        validator.Validate(request).IsValid.Should().BeTrue();
 
        var mapped = mapper.Map<OrderResult>(request);
        mapped.CustomerName.Should().Be("Grace Hopper");
        mapped.Amount.Should().Be(42m);
    }
}