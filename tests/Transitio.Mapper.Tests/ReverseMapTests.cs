#nullable enable
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Documents and locks in ReverseMap behaviour: the reverse map copies only plain
/// (simple) property mappings - custom ForMember(MapFrom) and Ignore from the forwards
/// map are intentionally NOT carried over.
/// </summary>
public class ReverseMapTests
{
    [Fact]
    public void ReverseMap_Does_Not_Carry_Custom_Member_Mapping()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.ToUpper()))
            .ReverseMap();
        });
        var mapper = config.BuildMapper();

        // Forward applies the custom ToUpper mapping.
        var dto = mapper.Map<UserDto>(new User { Name = "bob", Age = 5 });
        Assert.Equal("BOB", dto.Name);
        Assert.Equal(5, dto.Age);

        //Reverse copies as-is; the custom mapping is not carried over.
        var user = mapper.Map<User>(new UserDto { Name = "Alice", Age = 7 });
        Assert.Equal("Alice", user.Name);
        Assert.Equal(7, user.Age);
    }

    [Fact]
    public void ReverseMap_Does_Not_Carry_Ignore()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
            .ForMember(d => d.Age, opt => opt.Ignore())
            .ReverseMap();
        });
        var mapper = config.BuildMapper();

        //Forward ignore Age.
        var dto = mapper.Map<UserDto>(new User { Name = "x", Age = 9 });
        Assert.Equal(0, dto.Age);

        //Reverse does not ignore Age; it is copied normally.
        var user = mapper.Map<User>(new UserDto { Name = "y", Age = 11 });
        Assert.Equal(11, user.Age);
        Assert.Equal("y", user.Name);
    }
}