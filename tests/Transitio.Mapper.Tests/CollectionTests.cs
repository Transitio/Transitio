using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class CollectionTests
{
    [Fact]
    public void Should_Map_Collection()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        var users = new List<User>
        {
            new User { Name = "A", Age = 20 },
            new User { Name = "B", Age = 25 }
        };

        var result = mapper.Map<List<UserDto>>(users);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
    }
}