using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class MappingTests
{
    [Fact]
    public void Should_Map_User_To_UserDto()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Use_Custom_Mapping()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
               .Using(src => new UserDto
               {
                   Name = src.Name.ToUpper(),
                   Age = src.Age
               });
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("HITESH", result.Name);
    }
}