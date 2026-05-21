using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Xunit;

namespace Transitio.Dependency.Tests;

public class DependencyTests
{
    [Fact]
    public void Should_Resolve_TransitioDependency()
    {
        var services = new ServiceCollection();

        services.AddTransitio(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        var dep = provider.GetRequiredService<TransitioDependency>();

        Assert.NotNull(dep);
    }

    [Fact]
    public void Should_Resolve_Mapper_Through_Dependency()
    {
        var services = new ServiceCollection();

        services.AddTransitio(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        var dep = provider.GetRequiredService<TransitioDependency>();

        var mapper = dep.Mapping.Mapper;

        Assert.NotNull(mapper);
    }

    [Fact]
    public void Should_Map_Object_Using_Dependency()
    {
        var services = new ServiceCollection();

        services.AddTransitio(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        var dep = provider.GetRequiredService<TransitioDependency>();

        var result = dep.Mapping.Mapper.Map<UserDto>(
            new User { Name = "Hitesh", Age = 30 }
        );

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Use_Singleton_Dependency_Instance()
    {
        var services = new ServiceCollection();

        services.AddTransitio(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        var dep1 = provider.GetRequiredService<TransitioDependency>();
        var dep2 = provider.GetRequiredService<TransitioDependency>();

        Assert.Same(dep1, dep2);
    }
}