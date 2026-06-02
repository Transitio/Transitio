#nullable enable
using System.Collections.Generic;
using System.Linq;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class RecursiveMapperTests
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public List<Order>? Orders { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public List<OrderDto>? Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    [Fact]
    public void Should_Provide_Mapper_In_MappingContext()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        // Act
        var user = new User { Name = "John", Age = 30 };
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("John", userDto.Name);
        Assert.Equal(30, userDto.Age);
    }

    [Fact]
    public void Should_Allow_Recursive_Mapping_Via_Context()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Orders, opt => opt.MapFrom(
                    (s, ctx) => s.Orders?.Select(o => ctx.Mapper!.Map<OrderDto>(o)).ToList()
                ));
        });

        var mapper = config.BuildMapper();

        var user = new User
        {
            Name = "John",
            Age = 30,
            Orders = new List<Order>
            {
                new Order { Id = 1, Amount = 100.00m, Description = "Order 1" },
                new Order { Id = 2, Amount = 200.00m, Description = "Order 2" }
            }
        };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("John", userDto.Name);
        Assert.NotNull(userDto.Orders);
        Assert.Equal(2, userDto.Orders.Count);
        Assert.Equal(1, userDto.Orders[0].Id);
        Assert.Equal(100.00m, userDto.Orders[0].Amount);
        Assert.Equal("Order 1", userDto.Orders[0].Description);
        Assert.Equal(2, userDto.Orders[1].Id);
    }

    [Fact]
    public void Should_Support_Backward_Compatible_MapFrom_Without_Context()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.ToUpper()));
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "john", Age = 25 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("JOHN", userDto.Name);
        Assert.Equal(25, userDto.Age);
    }

    [Fact]
    public void Should_Allow_Conditional_Mapping_With_Context()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Age, opt => opt.MapFrom(
                    (s, ctx) => s.Age > 18 ? s.Age : 0
                ))
                .ForMember(d => d.Orders, opt => opt.MapFrom(
                    (s, ctx) => s.Orders != null ? s.Orders.Select(o => ctx.Mapper!.Map<OrderDto>(o)).ToList() : null
                ));

            cfg.CreateMap<Order, OrderDto>();
        });

        var mapper = config.BuildMapper();

        var user = new User
        {
            Name = "Alice",
            Age = 16,
            Orders = new List<Order>
            {
                new Order { Id = 1, Amount = 50.00m, Description = "Test" }
            }
        };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.Equal(0, userDto.Age);  // Age was 16, so mapped to 0
        Assert.NotNull(userDto.Orders);
        Assert.Single(userDto.Orders);
    }

    [Fact]
    public void Should_Access_Mapper_From_Context_In_Property_Mapping()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Orders, opt => opt.MapFrom(
                    (s, ctx) => s.Orders != null ? s.Orders.Select(o => ctx.Mapper!.Map<OrderDto>(o)).ToList() : null
                ));
        });

        var mapper = config.BuildMapper();

        var user = new User
        {
            Name = "Bob",
            Age = 35,
            Orders = new List<Order>
            {
                new Order { Id = 1, Amount = 150.00m, Description = "Premium" },
                new Order { Id = 2, Amount = 75.00m, Description = "Standard" }
            }
        };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.NotNull(userDto.Orders);
        Assert.Equal(2, userDto.Orders.Count);
        
        // Verify recursive mapping worked
        Assert.Equal(1, userDto.Orders[0].Id);
        Assert.Equal(150.00m, userDto.Orders[0].Amount);
        Assert.Equal("Premium", userDto.Orders[0].Description);
        
        Assert.Equal(2, userDto.Orders[1].Id);
        Assert.Equal(75.00m, userDto.Orders[1].Amount);
        Assert.Equal("Standard", userDto.Orders[1].Description);
    }

    [Fact]
    public void Should_Reuse_Mapper_Instance_For_Multiple_Mappings()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Orders, opt => opt.MapFrom(
                    (s, ctx) => s.Orders?.Select(o => ctx.Mapper!.Map<OrderDto>(o)).ToList()
                ));
        });

        var mapper = config.BuildMapper();

        var user1 = new User
        {
            Name = "User1",
            Age = 20,
            Orders = new List<Order> { new Order { Id = 1, Amount = 100m, Description = "Order1" } }
        };

        var user2 = new User
        {
            Name = "User2",
            Age = 25,
            Orders = new List<Order> { new Order { Id = 2, Amount = 200m, Description = "Order2" } }
        };

        // Act
        var userDto1 = mapper.Map<UserDto>(user1);
        var userDto2 = mapper.Map<UserDto>(user2);

        // Assert
        Assert.NotNull(userDto1);
        Assert.Single(userDto1.Orders ?? new List<OrderDto>());
        Assert.Equal(1, userDto1.Orders![0].Id);

        Assert.NotNull(userDto2);
        Assert.Single(userDto2.Orders ?? new List<OrderDto>());
        Assert.Equal(2, userDto2.Orders![0].Id);
    }

    [Fact]
    public void Should_Handle_Null_Collections_In_Recursive_Mapping()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<User, UserDto>()
                .ForMember(d => d.Orders, opt => opt.MapFrom(
                    (s, ctx) => s.Orders != null ? s.Orders.Select(o => ctx.Mapper!.Map<OrderDto>(o)).ToList() : null
                ));
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "NoOrders", Age = 40, Orders = null };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("NoOrders", userDto.Name);
        Assert.Null(userDto.Orders);
    }
}
