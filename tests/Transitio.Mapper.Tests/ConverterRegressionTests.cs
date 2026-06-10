#nullable enable
using System;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Regression tests for converter dispatch:
/// - explicitly-implemented ITypeConverter<,> must be invoked.
/// - converter exceptions must propagate.
/// </summary>
public class ConverterRegressionTests
{
    public class Source
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class Dest
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // Converter that implements the interface ExPLICITLY (no public convert method)
    public class ExplicitConverter : ITypeConverter<Source, Dest>
    {
        Dest ITypeConverter<Source, Dest>.Convert(Source source, IMappingContext context)
        => new Dest { Name = source.Name.ToUpper(), Value = source.Value + 1 };
    }

    public class ThrowingConverter : ITypeConverter<Source, Dest>
    {
        public Dest Convert(Source source, IMappingContext context)
        {
            throw new InvalidOperationException("boom from converter");
        }
    }

    [Fact]
    public void Should_Execute_Explicitly_Implemented_Instance_Converter()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
            .ConvertUsing(new ExplicitConverter());
        });

        var mapper = config.BuildMapper();
        var result = mapper.Map<Dest>(new Source { Name = "abc", Value = 10 });
        Assert.Equal("ABC", result.Name);
        Assert.Equal(11, result.Value);
    }

    [Fact]
    public void Should_Execute_Explicitly_Implemented_Type_Converter()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
            .ConvertUsing<ExplicitConverter>();
        });

        var mapper = config.BuildMapper();
        var result = mapper.Map<Dest>(new Source { Name = "xyz", Value = 5 });
        Assert.Equal("XYZ", result.Name);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public void Should_Propagate_Exception_From_Type_Converter()
    {
        var config = new TransitioMapperConfiguration(cgf =>
        {
            cgf.CreateMap<Source, Dest>()
            .ConvertUsing<ThrowingConverter>();
        });
        var mapper = config.BuildMapper();
        var ex = Assert.Throws<InvalidOperationException>(
            () => mapper.Map<Dest>(new Source { Name = "a", Value = 1 }));

        Assert.Equal("boom from converter", ex.Message);
    }

    [Fact]
    public void Should_Propagate_Exception_From_Delegate_Converter()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
            .ConvertUsing((Func<Source, IMappingContext, Dest>)((src, ctx) =>
            throw new InvalidOperationException("boom from delegate")));
        });

        var mapper = config.BuildMapper();
        var ex = Assert.Throws<InvalidOperationException>(
            () => mapper.Map<Dest>(new Source { Name = "a", Value = 1 }));

        Assert.Equal("boom from delegate", ex.Message);
    }
}