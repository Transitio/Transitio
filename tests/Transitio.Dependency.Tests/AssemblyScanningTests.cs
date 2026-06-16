using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that AddTransitio can discover and register mapping profiles by scanning
/// assemblies, instead of requiring every map to be declared inline.
/// </summary>
public class AssemblyScanningTests
{
    // A concrete profile that should be picked up by the scanner.
    public class ScannedUserProfile : MappingProfile
    {
        public override void Configure(TransitioConfigBuilder cfg)
        {
            cfg.CreateMap<User, UserDto>();
        }
    }

    // An abstract profile that must be ignored by the scanner.
    public abstract class AbstractProfile : MappingProfile
    {
    }

    [Fact]
    public void Should_Discover_Profiles_From_Assembly()
    {
        var services = new ServiceCollection();

        services.AddTransitio(Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Combine_Inline_Config_With_Assembly_Scanning()
    {
        var services = new ServiceCollection();

        // Inline config registers nothing extra here, but the assembly scan should still
        // supply the User -> UserDto map.
        services.AddTransitio(cfg => { }, Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;

        var result = mapper.Map<UserDto>(new User { Name = "Ada", Age = 42 });

        Assert.Equal("Ada", result.Name);
        Assert.Equal(42, result.Age);
    }

    [Fact]
    public void Should_Discover_Profiles_From_Assembly_Containing_Marker()
    {
        var services = new ServiceCollection();

        services.AddTransitio(Assembly.GetAssembly(typeof(ScannedUserProfile))!);

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;

        var result = mapper.Map<UserDto>(new User { Name = "Grace", Age = 25 });

        Assert.Equal("Grace", result.Name);
        Assert.Equal(25, result.Age);
    }
}
