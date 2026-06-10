#nullable enable
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Edge cases for SetIgnoreNullValues: null source values preserve the destination's
/// default, non-null values are still written, and the default (off) overwrites with null.
/// Covers both reference (string) and nullable value-type (int?) members.
/// </summary>
public class IgnoreNullValuesTests
{
    public class Source
    {
        public string? Name { get; set; }
        public int? Count { get; set; }
    }
    public class Dest
    {
        public string Name { get; set; } = "Default";
        public int? Count { get; set; } = 42;
    }

    [Fact]
    public void IgnoreNullValues_Off_Overwrites_Destination_Default_With_Null()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });
        var mapper = config.BuildMapper();

        var result = mapper.Map<Dest>(new Source { Name = null, Count = null });

        Assert.Null(result.Name);
        Assert.Null(result.Count);
    }

    [Fact]
    public void IgnoreNullValues_On_Preserves_Destination_Defaults_For_Null_Source()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.SetIgnoreNullValues(true);
            cfg.CreateMap<Source, Dest>();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<Dest>(new Source { Name = null, Count = null });

        Assert.Equal("Default", result.Name);
        Assert.Equal(42, result.Count);
    }

    [Fact]
    public void IgnoreNullValues_On_Still_Writes_NonNull_Source_Values()
    {
        var config = new TransitioMapperConfiguration(cfg =>
       {
           cfg.SetIgnoreNullValues(true);
           cfg.CreateMap<Source, Dest>();
       });

        var mapper = config.BuildMapper();

        var result = mapper.Map<Dest>(new Source { Name = "Real", Count = 7 });

        Assert.Equal("Real", result.Name);
        Assert.Equal(7, result.Count);
    }
}