using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class ProfileTests
{
    [Fact]
    public void Should_Map_Using_Profile()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
    }
}