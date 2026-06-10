#nullable enable
using System;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Runtime error paths: a null source is rejected, and mapping an unconfigured type pair
/// fails fast with a descriptive error.
/// </summary>
public class ErrorHandlingTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_For_Null_Source()
    {
        var config = new TransitioMapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        var mapper = config.BuildMapper();

        Assert.Throws<ArgumentNullException>(() => mapper.Map<UserDto>(null!));
    }

    [Fact]
    public void Should_Throw_When_Mapping_Not_Configured()
    {
        // No CreatMap registered at all.
        var config = new TransitioMapperConfiguration(_ => { });
        var mapper = config.BuildMapper();

        var ex = Assert.Throws<InvalidOperationException>(
            () => mapper.Map<UserDto>(new User { Name = "a", Age = 1 }));

        Assert.Contains("Mapping not found.", ex.Message);
    }
}