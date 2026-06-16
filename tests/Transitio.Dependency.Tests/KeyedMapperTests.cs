using System;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that multiple independent mapper configurations can be registered side by
/// side under different keys and resolved via keyed services.
/// </summary>
public class KeyedMapperTests
{
    [Fact]
    public void Should_Resolve_Independent_Mappers_By_Key()
    {
        var services = new ServiceCollection();

        // "upper" maps Name to upper-case via a delegate converter.
        services.AddKeyedTransitio("upper", cfg =>
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((src, _) => new UserDto { Name = src.Name.ToUpperInvariant(), Age = src.Age }));

        // "plain" is a straight property copy.
        services.AddKeyedTransitio("plain", cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider();

        var upper = provider.GetRequiredKeyedService<IMapper>("upper");
        var plain = provider.GetRequiredKeyedService<IMapper>("plain");

        var source = new User { Name = "Hitesh", Age = 30 };

        Assert.Equal("HITESH", upper.Map<UserDto>(source).Name);
        Assert.Equal("Hitesh", plain.Map<UserDto>(source).Name);
    }

    [Fact]
    public void Keyed_Mappers_Are_Distinct_Instances()
    {
        var services = new ServiceCollection();
        services.AddKeyedTransitio("a", cfg => cfg.CreateMap<User, UserDto>());
        services.AddKeyedTransitio("b", cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider();

        var a = provider.GetRequiredKeyedService<IMapper>("a");
        var b = provider.GetRequiredKeyedService<IMapper>("b");

        Assert.NotSame(a, b);
    }

    [Fact]
    public void Keyed_And_NonKeyed_Registrations_Coexist()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
        services.AddKeyedTransitio("special", cfg =>
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((src, _) => new UserDto { Name = "special", Age = src.Age }));

        var provider = services.BuildServiceProvider();

        var defaultMapper = provider.GetRequiredService<IMapper>();
        var special = provider.GetRequiredKeyedService<IMapper>("special");

        var source = new User { Name = "Ada", Age = 42 };

        Assert.Equal("Ada", defaultMapper.Map<UserDto>(source).Name);
        Assert.Equal("special", special.Map<UserDto>(source).Name);
    }

    [Fact]
    public void Keyed_Mapper_Honors_Scoped_Lifetime()
    {
        var services = new ServiceCollection();
        services.AddKeyedTransitio("scoped", ServiceLifetime.Scoped,
            cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider(validateScopes: true);

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredKeyedService<IMapper>("scoped");
        var result = mapper.Map<UserDto>(new User { Name = "Grace", Age = 25 });

        Assert.Equal("Grace", result.Name);
        Assert.Equal(25, result.Age);
    }

    [Fact]
    public void Null_Key_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddKeyedTransitio(null!, cfg => cfg.CreateMap<User, UserDto>()));
    }
}
