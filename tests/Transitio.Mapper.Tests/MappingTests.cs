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

    [Fact]
    public void Should_Use_ForMember_Custom_Mapping()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToUpper()));
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("HITESH", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Ignore_Property_When_Configured()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Ignore());
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public void Should_Apply_Condition_To_Member()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age > 18));
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 16 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public void Should_Ignore_Null_Source_Values_When_Configured()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.SetIgnoreNullValues(true);
            cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserWithDefaultNameDto>(new UserWithNullableName { Name = null });

        Assert.Equal("Default", result.Name);
    }

    [Fact]
    public void Should_Support_ReverseMap()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>().ReverseMap();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<User>(new UserDto { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    private class UserWithNullableName
    {
        public string? Name { get; set; }
    }

    private class UserWithDefaultNameDto
    {
        public string Name { get; set; } = "Default";
    }
}