using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;
 
namespace Transitio.Dependency.Tests;
 
/// <summary>
/// Verifies TryAddTransitio's idempotent (first-call-wins) registration, as opposed to
/// AddTransitio's stacking (last-call-wins) behavior.
/// </summary>
public class TryAddTransitioTests
{
    // Has a property (Email) with no matching source property on User, so ValidateConfiguration()
    // rejects it — used to prove a skipped TryAddTransitio call never builds its configuration.
    public class UserWithEmailDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
    }
 
    [Fact]
    public void Registers_When_Nothing_Registered_Yet()
    {
        var services = new ServiceCollection();
        services.TryAddTransitio(cfg => cfg.CreateMap<User, UserDto>());
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
 
        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });
        Assert.Equal("Hitesh", result.Name);
    }
 
    [Fact]
    public void Is_A_NoOp_When_AddTransitio_Already_Registered()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
        services.TryAddTransitio(cfg =>
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((src, _) => new UserDto { Name = "overridden", Age = src.Age }));
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
 
        // The first registration's configuration wins; the second (skipped) call's
        // ConvertUsing override never takes effect.
        Assert.Equal("Hitesh", mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 }).Name);
    }
 
    [Fact]
    public void Called_Twice_Only_Registers_Once()
    {
        var services = new ServiceCollection();
        services.TryAddTransitio(cfg => cfg.CreateMap<User, UserDto>());
        services.TryAddTransitio(cfg =>
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((src, _) => new UserDto { Name = "overridden", Age = src.Age }));
 
        Assert.Single(services, d => d.ServiceType == typeof(IMapper));
    }
 
    [Fact]
    public void Works_With_Assembly_Scanning_Overload()
    {
        var services = new ServiceCollection();
        services.TryAddTransitio(System.Reflection.Assembly.GetExecutingAssembly());
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;
 
        var result = mapper.Map<UserDto>(new User { Name = "Ada", Age = 42 });
        Assert.Equal("Ada", result.Name);
    }
 
    [Fact]
    public void Keyed_Registers_First_And_Skips_Second_For_Same_Key()
    {
        var services = new ServiceCollection();
        services.TryAddKeyedTransitio("shared", cfg => cfg.CreateMap<User, UserDto>());
        services.TryAddKeyedTransitio("shared", cfg =>
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((src, _) => new UserDto { Name = "overridden", Age = src.Age }));
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredKeyedService<IMapper>("shared");
 
        Assert.Equal("Grace", mapper.Map<UserDto>(new User { Name = "Grace", Age = 25 }).Name);
    }
 
    [Fact]
    public void Keyed_Allows_Different_Keys_Independently()
    {
        var services = new ServiceCollection();
        services.TryAddKeyedTransitio("a", cfg => cfg.CreateMap<User, UserDto>());
        services.TryAddKeyedTransitio("b", cfg => cfg.CreateMap<User, UserDto>());
 
        var provider = services.BuildServiceProvider();
 
        Assert.NotNull(provider.GetRequiredKeyedService<IMapper>("a"));
        Assert.NotNull(provider.GetRequiredKeyedService<IMapper>("b"));
    }
 
    [Fact]
    public void Skipped_Configuration_Is_Not_Built_When_Already_Registered()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
 
        // A second call whose configuration would fail validation if it were built at all —
        // this must not throw, because the call is skipped entirely (IMapper is already
        // registered), not built-then-discarded.
        services.TryAddTransitio(cfg =>
        {
            cfg.ValidateConfiguration();
            cfg.CreateMap<User, UserWithEmailDto>(); // Email has no matching source property.
        });
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
 
        Assert.Equal("Hitesh", mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 }).Name);
    }
 
    [Fact]
    public void Keyed_Skipped_Configuration_Is_Not_Built_When_Key_Already_Registered()
    {
        var services = new ServiceCollection();
        services.AddKeyedTransitio("shared", cfg => cfg.CreateMap<User, UserDto>());
 
        services.TryAddKeyedTransitio("shared", cfg =>
        {
            cfg.ValidateConfiguration();
            cfg.CreateMap<User, UserWithEmailDto>();
        });
 
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredKeyedService<IMapper>("shared");
 
        Assert.Equal("Grace", mapper.Map<UserDto>(new User { Name = "Grace", Age = 25 }).Name);
    }
 
    [Fact]
    public void Lifetime_Overload_Honors_Lifetime()
    {
        var services = new ServiceCollection();
        services.TryAddTransitio(ServiceLifetime.Transient, cfg => cfg.CreateMap<User, UserDto>());
 
        var provider = services.BuildServiceProvider();
 
        var dep1 = provider.GetRequiredService<TransitioDependency>();
        var dep2 = provider.GetRequiredService<TransitioDependency>();
 
        Assert.NotSame(dep1, dep2);
    }
}