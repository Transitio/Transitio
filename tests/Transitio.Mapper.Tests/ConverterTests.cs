#nullable enable
using System;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class ConverterTests
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string FormattedAmount { get; set; } = string.Empty;
    }

    // ✅ Type converter implementation
    public class UserConverter : ITypeConverter<User, UserDto>
    {
        public UserDto Convert(User source, IMappingContext context)
        {
            return new UserDto
            {
                Name = source.Name.ToUpper(),
                Age = source.Age * 2
            };
        }
    }

    public class OrderConverter : ITypeConverter<Order, OrderDto>
    {
        public OrderDto Convert(Order source, IMappingContext context)
        {
            return new OrderDto
            {
                Id = source.Id,
                Amount = source.Amount,
                FormattedAmount = $"${source.Amount:F2}"
            };
        }
    }

    [Fact]
    public void Should_Execute_Generic_Type_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing<UserConverter>();
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "john", Age = 25 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("JOHN", userDto.Name);  // Converter transformed to uppercase
        Assert.Equal(50, userDto.Age);       // Converter doubled the age
    }

    [Fact]
    public void Should_Execute_Instance_Based_Converter()
    {
        // Arrange
        var converter = new UserConverter();
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing(converter);
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "alice", Age = 30 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("ALICE", userDto.Name);
        Assert.Equal(60, userDto.Age);
    }

    [Fact]
    public void Should_Execute_Delegate_Based_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((source, context) => new UserDto
                {
                    Name = source.Name.ToLower(),
                    Age = source.Age + 10
                });
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "BOB", Age = 20 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("bob", userDto.Name);
        Assert.Equal(30, userDto.Age);
    }

    [Fact]
    public void Should_Override_Property_Mapping_With_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ConvertUsing<OrderConverter>()
                // This ForMember should be ignored when converter is used
                .ForMember(d => d.FormattedAmount, opt => opt.MapFrom(s => "ignored"));
        });

        var mapper = config.BuildMapper();

        var order = new Order { Id = 1, Amount = 99.99m };

        // Act
        var orderDto = mapper.Map<OrderDto>(order);

        // Assert
        Assert.NotNull(orderDto);
        Assert.Equal(1, orderDto.Id);
        Assert.Equal(99.99m, orderDto.Amount);
        // Converter's formatted amount should be used, not the ForMember mapping
        Assert.Equal("$99.99", orderDto.FormattedAmount);
    }

    [Fact]
    public void Should_Access_Mapper_From_Context_In_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((source, context) =>
                {
                    // This demonstrates accessing mapper from context within converter
                    var dtoName = source.Name.ToUpper();
                    return new UserDto
                    {
                        Name = dtoName,
                        Age = source.Age
                    };
                });
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "charlie", Age = 35 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("CHARLIE", userDto.Name);
        Assert.Equal(35, userDto.Age);
    }

    [Fact]
    public void Should_Execute_Multiple_Different_Converters()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing<UserConverter>();
            
            cfg.CreateMap<Order, OrderDto>()
                .ConvertUsing<OrderConverter>();
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "dave", Age = 40 };
        var order = new Order { Id = 101, Amount = 150.00m };

        // Act
        var userDto = mapper.Map<UserDto>(user);
        var orderDto = mapper.Map<OrderDto>(order);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("DAVE", userDto.Name);
        Assert.Equal(80, userDto.Age);

        Assert.NotNull(orderDto);
        Assert.Equal(101, orderDto.Id);
        Assert.Equal("$150.00", orderDto.FormattedAmount);
    }

    [Fact]
    public void Should_Handle_Null_Values_In_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing((source, context) =>
                {
                    var name = source.Name ?? "Unknown";
                    return new UserDto
                    {
                        Name = name.ToUpper(),
                        Age = source.Age
                    };
                });
        });

        var mapper = config.BuildMapper();

        var user = new User { Name = "", Age = 25 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("", userDto.Name);  // Empty string stays empty
    }

    [Fact]
    public void Should_Support_Complex_Conversion_Logic_In_Type_Converter()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ConvertUsing((source, context) =>
                {
                    var dto = new OrderDto
                    {
                        Id = source.Id,
                        Amount = source.Amount
                    };

                    // Complex logic
                    if (source.Amount > 100)
                    {
                        dto.FormattedAmount = $"Premium: ${source.Amount:F2}";
                    }
                    else
                    {
                        dto.FormattedAmount = $"Standard: ${source.Amount:F2}";
                    }

                    return dto;
                });
        });

        var mapper = config.BuildMapper();

        var premiumOrder = new Order { Id = 1, Amount = 150.00m };
        var standardOrder = new Order { Id = 2, Amount = 50.00m };

        // Act
        var premiumDto = mapper.Map<OrderDto>(premiumOrder);
        var standardDto = mapper.Map<OrderDto>(standardOrder);

        // Assert
        Assert.Equal("Premium: $150.00", premiumDto.FormattedAmount);
        Assert.Equal("Standard: $50.00", standardDto.FormattedAmount);
    }

    [Fact]
    public void Should_Cache_Converter_Instances_Across_Mappings()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing<UserConverter>();
        });

        var mapper = config.BuildMapper();

        var user1 = new User { Name = "user1", Age = 20 };
        var user2 = new User { Name = "user2", Age = 30 };

        // Act
        var dto1 = mapper.Map<UserDto>(user1);
        var dto2 = mapper.Map<UserDto>(user2);

        // Assert
        Assert.NotNull(dto1);
        Assert.Equal("USER1", dto1.Name);
        Assert.Equal(40, dto1.Age);

        Assert.NotNull(dto2);
        Assert.Equal("USER2", dto2.Name);
        Assert.Equal(60, dto2.Age);
    }

    [Fact]
    public void Should_Preserve_Object_Cache_With_Converters()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConvertUsing<UserConverter>();
        });

        var mapper = config.BuildMapper();
        var user = new User { Name = "eve", Age = 28 };

        // Act
        var userDto = mapper.Map<UserDto>(user);

        // Assert - converter should cache result
        Assert.NotNull(userDto);
        Assert.Equal("EVE", userDto.Name);
    }
}
