using System;
using Transitio.Mapper;
using Xunit;
 
namespace Transitio.Mapper.Tests;
 
public class MapIntoExistingInstanceTests
{
    public class UserConverter : ITypeConverter<User, UserDto>
    {
        public UserDto Convert(User source, IMappingContext context)
            => new() { Name = source.Name.ToUpperInvariant(), Age = source.Age };
    }
 
    [Fact]
    public void Populates_Flat_Properties_And_Returns_Same_Reference()
    {
        var config = new TransitioMapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        var mapper = config.BuildMapper();
 
        var dto = new UserDto();
        var result = mapper.Map(new User { Name = "Hitesh", Age = 30 }, dto);
 
        Assert.Same(dto, result);
        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }
 
    [Fact]
    public void Overwrites_Previously_Set_Destination_Values()
    {
        var config = new TransitioMapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        var mapper = config.BuildMapper();
 
        var dto = new UserDto { Name = "Stale", Age = 99 };
        mapper.Map(new User { Name = "Fresh", Age = 21 }, dto);
 
        Assert.Equal("Fresh", dto.Name);
        Assert.Equal(21, dto.Age);
    }
 
    [Fact]
    public void Applies_ForMember_MapFrom()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToUpper())));
        var mapper = config.BuildMapper();
 
        var dto = new UserDto();
        mapper.Map(new User { Name = "Hitesh", Age = 30 }, dto);
 
        Assert.Equal("HITESH", dto.Name);
        Assert.Equal(30, dto.Age);
    }
 
    [Fact]
    public void Applies_Ignore()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Ignore()));
        var mapper = config.BuildMapper();
 
        var dto = new UserDto();
        mapper.Map(new User { Name = "Hitesh", Age = 30 }, dto);
 
        Assert.Equal("Hitesh", dto.Name);
        Assert.Equal(0, dto.Age);
    }
 
    [Fact]
    public void Ignored_Property_Preserves_Destinations_PreExisting_Value()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Ignore()));
        var mapper = config.BuildMapper();
 
        var dto = new UserDto { Age = 42 };
        mapper.Map(new User { Name = "Hitesh", Age = 99 }, dto);
 
        Assert.Equal("Hitesh", dto.Name);
        Assert.Equal(42, dto.Age);
    }
 
    [Fact]
    public void Failing_Condition_Preserves_Destinations_PreExisting_Value()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age > 18)));
        var mapper = config.BuildMapper();
 
        var dto = new UserDto { Age = 42 };
        mapper.Map(new User { Name = "Hitesh", Age = 16 }, dto);
 
        Assert.Equal("Hitesh", dto.Name);
        Assert.Equal(42, dto.Age);
    }
 
    [Fact]
    public void Passing_Condition_Still_Writes_The_New_Value()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>()
               .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age > 18)));
        var mapper = config.BuildMapper();
 
        var dto = new UserDto { Age = 42 };
        mapper.Map(new User { Name = "Hitesh", Age = 30 }, dto);
 
        Assert.Equal(30, dto.Age);
    }
 
    [Fact]
    public void Respects_SetIgnoreNullValues()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.SetIgnoreNullValues(true);
            cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();
        });
        var mapper = config.BuildMapper();
 
        var dto = new UserWithDefaultNameDto { Name = "Kept" };
        mapper.Map(new UserWithNullableName { Name = null }, dto);
 
        Assert.Equal("Kept", dto.Name);
    }
 
    [Fact]
    public void Maps_Nested_Objects()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.BuildMapper();
 
        var order = new Order { Id = "o1", Customer = new User { Name = "Hitesh", Age = 30 } };
        var dto = new OrderDto();
        mapper.Map(order, dto);
 
        Assert.Equal("o1", dto.Id);
        Assert.Equal("Hitesh", dto.Customer.Name);
        Assert.Equal(30, dto.Customer.Age);
    }
 
    [Fact]
    public void Throws_For_Unregistered_Mapping()
    {
        var config = new TransitioMapperConfiguration(cfg => { });
        var mapper = config.BuildMapper();
 
        Assert.Throws<InvalidOperationException>(() => mapper.Map(new User(), new UserDto()));
    }
 
    [Fact]
    public void Throws_For_ConvertUsing_Converter()
    {
        var config = new TransitioMapperConfiguration(cfg =>
            cfg.CreateMap<User, UserDto>().ConvertUsing<UserConverter>());
        var mapper = config.BuildMapper();
 
        Assert.Throws<InvalidOperationException>(() => mapper.Map(new User { Name = "Hitesh" }, new UserDto()));
    }
 
    [Fact]
    public void Throws_On_Null_Source_Or_Destination()
    {
        var config = new TransitioMapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        var mapper = config.BuildMapper();
 
        Assert.Throws<ArgumentNullException>(() => mapper.Map<User, UserDto>(null!, new UserDto()));
        Assert.Throws<ArgumentNullException>(() => mapper.Map(new User(), (UserDto)null!));
    }
 
    // A minimal IMapper implementation that does not override the new default interface member,
    // proving the default Map<TSource,TDestination> compiles and works for pre-existing
    // implementers (non-breaking-ness check).
    private sealed class MinimalMapper : IMapper
    {
        public TDestination Map<TDestination>(object source)
        {
            var dest = Activator.CreateInstance<TDestination>()!;
            foreach (var prop in typeof(TDestination).GetProperties())
            {
                var sourceProp = source.GetType().GetProperty(prop.Name);
                if (sourceProp != null && prop.CanWrite)
                    prop.SetValue(dest, sourceProp.GetValue(source));
            }
            return dest;
        }
    }
 
    [Fact]
    public void Default_Interface_Implementation_Works_For_A_Minimal_IMapper()
    {
        IMapper mapper = new MinimalMapper();
 
        var dto = new UserDto();
        var result = mapper.Map(new User { Name = "Hitesh", Age = 30 }, dto);
 
        Assert.Same(dto, result);
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