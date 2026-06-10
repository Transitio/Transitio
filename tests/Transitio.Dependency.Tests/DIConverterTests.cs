using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that type-based converters registered with ConvertUsing<TConverter>()
/// are instantiated through the DI container, so they can declare constructor dependencies.
/// </summary>
public class DIConverterTests
{
    public class Number { public int Value { get; set; } }
    public class NumberDto { public int Value { get; set; } }

    public class MultiplierService
    {
        public int Factor => 3;
    }

    public class MultiplyConverter : ITypeConverter<Number, NumberDto>
    {
        private readonly MultiplierService _service;

        public MultiplyConverter(MultiplierService service) => _service = service;

        public NumberDto Convert(Number source, IMappingContext context)
        => new NumberDto { Value = source.Value * _service.Factor };
    }

    [Fact]
    public void Should_Resolve_Type_Converter_With_Constructor_Dependencies()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MultiplierService>();
        services.AddTransitio(cfg =>
        {
            cfg.CreateMap<Number, NumberDto>().ConvertUsing<MultiplyConverter>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;
        var result = mapper.Map<NumberDto>(new Number { Value = 5 });
        Assert.Equal(15, result.Value); // 5 * Factor(3)
    }
}