using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that AddTransitio honors the requested service lifetime, and that scoped
/// lifetimes let a ConvertUsing converter resolve scoped constructor dependencies from
/// the same scope as the mapper (no captive dependency on the root provider).
/// </summary>
public class LifetimeTests
{
    public class Number { public int Value { get; set; } }
    public class NumberDto { public int Value { get; set; } }

    // A scoped service: each scope gets a distinct instance with its own tag.
    public class ScopedTagService
    {
        public string Tag { get; set; } = "";
    }

    // Converter depends on a scoped service; with a scoped mapper it must observe the
    // tag belonging to the scope it was resolved from.
    public class TaggingConverter : ITypeConverter<Number, NumberDto>
    {
        private readonly ScopedTagService _service;

        public TaggingConverter(ScopedTagService service) => _service = service;

        public NumberDto Convert(Number source, IMappingContext context)
            => new NumberDto { Value = source.Value + _service.Tag.Length };
    }

    [Fact]
    public void Should_Resolve_Scoped_Converter_Dependency_From_Owning_Scope()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedTagService>();
        services.AddTransitio(
            ServiceLifetime.Scoped,
            cfg => cfg.CreateMap<Number, NumberDto>().ConvertUsing<TaggingConverter>());

        // validateScopes:true makes resolving a scoped service from the root throw,
        // proving the mapper is genuinely scope-bound.
        var provider = services.BuildServiceProvider(validateScopes: true);

        using (var scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ScopedTagService>().Tag = "abcd"; // len 4
            var mapper = scope.ServiceProvider.GetRequiredService<TransitioDependency>().Mapping.Mapper;
            var result = mapper.Map<NumberDto>(new Number { Value = 10 });
            Assert.Equal(14, result.Value);
        }

        using (var scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ScopedTagService>().Tag = "x"; // len 1
            var mapper = scope.ServiceProvider.GetRequiredService<TransitioDependency>().Mapping.Mapper;
            var result = mapper.Map<NumberDto>(new Number { Value = 10 });
            Assert.Equal(11, result.Value);
        }
    }

    [Fact]
    public void Should_Give_Different_TransitioDependency_Instances_Per_Scope_When_Scoped()
    {
        var services = new ServiceCollection();
        services.AddTransitio(
            ServiceLifetime.Scoped,
            cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider(validateScopes: true);

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var dep1 = scope1.ServiceProvider.GetRequiredService<TransitioDependency>();
        var dep2 = scope2.ServiceProvider.GetRequiredService<TransitioDependency>();

        Assert.NotSame(dep1, dep2);
    }

    [Fact]
    public void Should_Give_New_TransitioDependency_Each_Resolve_When_Transient()
    {
        var services = new ServiceCollection();
        services.AddTransitio(
            ServiceLifetime.Transient,
            cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider();

        var dep1 = provider.GetRequiredService<TransitioDependency>();
        var dep2 = provider.GetRequiredService<TransitioDependency>();

        Assert.NotSame(dep1, dep2);
    }

    [Fact]
    public void Should_Default_To_Singleton()
    {
        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<User, UserDto>());

        var provider = services.BuildServiceProvider();

        var dep1 = provider.GetRequiredService<TransitioDependency>();
        var dep2 = provider.GetRequiredService<TransitioDependency>();

        Assert.Same(dep1, dep2);
    }
}
