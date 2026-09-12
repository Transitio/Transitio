using Microsoft.Extensions.DependencyInjection;
using Transitio.Assertions;
using Transitio.Dependency;
using Transitio.Mapper;
using Transitio.Mediator;
using Transitio.Validation;
using Xunit;
 
namespace Transitio.Integration.Tests;
 
/// <summary>
/// Simulates two "modules" composing Transitio into one shared container without knowing about
/// each other — the scenario <c>TryAddTransitio</c> (Milestone 1) exists for — alongside
/// Transitio.Validation and Transitio.Mediator registered the normal way, proving the idempotent
/// registration doesn't disturb the other packages' wiring in the same container.
/// </summary>
public class MultiPackageCompositionTests
{
    [Fact]
    public async Task First_modules_mapping_wins_and_the_rest_of_the_stack_still_works()
    {
        var services = new ServiceCollection();
 
        // "Module A" registers Transitio.Mapper first...
        services.TryAddTransitio(cfg => cfg.CreateMap<PlaceOrderRequest, OrderResult>());
 
        // ...alongside Validation and Mediator, registered the ordinary way.
        services.AddTransitioValidation(typeof(PlaceOrderValidator).Assembly);
        services.AddTransitioMediator(typeof(PlaceOrderHandler).Assembly);
        services.AddSingleton<OrderProcessingTracker>();
 
        // "Module B" doesn't know Module A already configured Transitio, and tries again with a
        // deliberately different mapping. This call must be skipped entirely.
        services.TryAddTransitio(cfg =>
            cfg.CreateMap<PlaceOrderRequest, OrderResult>()
               .ForMember(d => d.CustomerName, o => o.MapFrom(_ => "should-not-apply")));
 
        using var provider = services.BuildServiceProvider();
 
        // Module A's mapping is the one that's actually active.
        var mapper = provider.GetRequiredService<IMapper>();
        mapper.Map<OrderResult>(new PlaceOrderRequest { CustomerName = "Grace", Amount = 10m })
            .CustomerName.Should().Be("Grace");
 
        // The rest of the stack composed alongside it is unaffected: a full Send still runs the
        // validation behavior and handler correctly.
        var mediator = provider.GetRequiredService<IMediator>();
        var tracker = provider.GetRequiredService<OrderProcessingTracker>();
 
        var result = await mediator.Send(new PlaceOrderRequest
        {
            CustomerName = "Grace",
            Email = "grace@example.com",
            Amount = 50m,
        });
 
        result.CustomerName.Should().Be("Grace");
        tracker.ProcessedCount.Should().Be(1);
    }
 
    [Fact]
    public void Keyed_and_default_mappers_coexist_alongside_validation_and_mediator_registrations()
    {
        var services = new ServiceCollection();
 
        services.AddTransitio(cfg => cfg.CreateMap<PlaceOrderRequest, OrderResult>());
        services.AddKeyedTransitio("audit", cfg =>
            cfg.CreateMap<PlaceOrderRequest, OrderResult>()
               .ForMember(d => d.Status, o => o.MapFrom(_ => "Audited")));
        services.AddTransitioValidation(typeof(PlaceOrderValidator).Assembly);
        services.AddTransitioMediator(typeof(PlaceOrderHandler).Assembly);
 
        using var provider = services.BuildServiceProvider();
 
        var defaultMapper = provider.GetRequiredService<IMapper>();
        var auditMapper = provider.GetRequiredKeyedService<IMapper>("audit");
        var validator = provider.GetRequiredService<IValidator<PlaceOrderRequest>>();
        var mediator = provider.GetRequiredService<IMediator>();
 
        var request = new PlaceOrderRequest { CustomerName = "Ada", Email = "ada@example.com", Amount = 5m };
 
        defaultMapper.Map<OrderResult>(request).Status.Should().Be("");
        auditMapper.Map<OrderResult>(request).Status.Should().Be("Audited");
        validator.Validate(request).IsValid.Should().BeTrue();
        mediator.Should().NotBeNull();
    }
}