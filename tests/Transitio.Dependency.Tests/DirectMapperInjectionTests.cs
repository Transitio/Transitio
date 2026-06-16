using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that IMapper can be resolved and constructor-injected directly, without going
/// through TransitioDependency.Mapping.Mapper.
/// </summary>
public class DirectMapperInjectionTests
{
    // A consumer that takes a dependency on IMapper directly.
    public class UserService
    {
        private readonly IMapper _mapper;

        public UserService(IMapper mapper) => _mapper = mapper;

        public UserDto ToDto(User user) => _mapper.Map<UserDto>(user);
    }

    [Fact]
    public void Should_Resolve_IMapper_Directly()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Constructor_Inject_IMapper_Into_Consumer()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());
        services.AddSingleton<UserService>();

        var provider = services.BuildServiceProvider();
        var consumer = provider.GetRequiredService<UserService>();

        var result = consumer.ToDto(new User { Name = "Ada", Age = 42 });

        Assert.Equal("Ada", result.Name);
        Assert.Equal(42, result.Age);
    }
}
