using Microsoft.Extensions.DependencyInjection;

namespace Transitio.Validation.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddTransitioValidation_Registers_Closed_Validator()
    {
        var provider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();

        var validator = provider.GetService<IValidator<Customer>>();

        Assert.NotNull(validator);
        Assert.IsType<CustomerValidator>(validator);
    }

    [Fact]
    public void Registered_Validator_Works()
    {
        var provider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();

        var validator = provider.GetRequiredService<IValidator<Customer>>();

        Assert.False(validator.Validate(new Customer { Name = "", Email = "x", Age = -1 }).IsValid);
    }

    [Fact]
    public void Default_Lifetime_Is_Singleton()
    {
        var services = new ServiceCollection();
        services.AddTransitioValidation(typeof(CustomerValidator).Assembly);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IValidator<Customer>));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void Supplied_Lifetime_Is_Respected()
    {
        var services = new ServiceCollection();
        services.AddTransitioValidation(ServiceLifetime.Scoped, typeof(CustomerValidator).Assembly);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IValidator<Customer>));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void Closed_Interface_And_Concrete_Resolve_To_Same_Instance()
    {
        var provider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();

        var viaInterface = provider.GetRequiredService<IValidator<Customer>>();
        var viaConcrete = provider.GetRequiredService<CustomerValidator>();

        Assert.Same(viaConcrete, viaInterface);
    }
}
