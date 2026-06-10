#nullable enable
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Regression tests ensuring AssertConfigurationIsValid does not raise false positives for
/// maps that legitimately have no 1:1 property correspondence: full converters, custom
/// ForMember(MapFrom), and Ignore.
/// </summary>
public class ValidationAdvancedTests
{
    public class Src
    {
        public string Name { get; set; } = string.Empty;
    }

    //Dst has an extra property not present on the source.
    public class Dst
    {
        public string Name { get; set; } = string.Empty;
        public string Computed { get; set; } = string.Empty;
    }

    public class SrcToDstConverter : ITypeConverter<Src, Dst>
    {
        public Dst Convert(Src source, IMappingContext context)
        => new Dst { Name = source.Name, Computed = source.Name.Length.ToString() };
    }

    [Fact]
    public void Should_Not_Flag_Converter_Based_Map()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Src, Dst>().ConvertUsing<SrcToDstConverter>();
        });

        var exception = Record.Exception(() => config.AssertConfigurationIsValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Should_Not_Flag_Member_Mapped_With_ForMember()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Src, Dst>()
            .ForMember(d => d.Computed, opt => opt.MapFrom(s => s.Name.Length.ToString()));
        });


        var exception = Record.Exception(() => config.AssertConfigurationIsValid());

        Assert.Null(exception);
    }

    [Fact]
    public void Should_Not_Flag_Ignored_Member()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Src, Dst>()
            .ForMember(d => d.Computed, opt => opt.Ignore());
        });

        var exception = Record.Exception(() => config.AssertConfigurationIsValid());

        Assert.Null(exception);
    }

    [Fact]
    public void Should_Still_Flag_Genuinely_Unwrapped_Member()
    {
        // No converter, no ForMember/Ignore for Computed -> should still report it.
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Src, Dst>();
        });

        var ex = Assert.Throws<System.InvalidOperationException>(
            () => config.AssertConfigurationIsValid());


        Assert.Contains("Missing source property", ex.Message);
    }
}