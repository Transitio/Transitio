using System;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class ValidationTests
{
    // ✅ 1. Valid mapping should PASS
    [Fact]
    public void Should_Not_Throw_For_Valid_Mapping()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var exception = Record.Exception(() =>
            config.AssertConfigurationIsValid());

        Assert.Null(exception);
    }

    // ✅ 2. Missing source property should FAIL
    [Fact]
    public void Should_Throw_For_Missing_Source_Property()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, BadDto>();
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            config.AssertConfigurationIsValid());

        Assert.Contains("Missing source property", ex.Message);
    }

    // ✅ 3. Type mismatch should FAIL
    [Fact]
    public void Should_Throw_For_Type_Mismatch()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, WrongTypeDto>();
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            config.AssertConfigurationIsValid());

        Assert.Contains("Type mismatch", ex.Message);
    }

    // ✅ 4. Nested mapping should PASS if mapping exists
    [Fact]
    public void Should_Validate_Nested_Mapping_When_Configured()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<User, UserDto>();
        });

        var exception = Record.Exception(() =>
            config.AssertConfigurationIsValid());

        Assert.Null(exception);
    }

    // ✅ 5. Nested mapping should FAIL if missing mapping
    [Fact]
    public void Should_Throw_When_Nested_Mapping_Is_Missing()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            // ❌ Missing: cfg.CreateMap<User, UserDto>();
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            config.AssertConfigurationIsValid());

        Assert.Contains("Type mismatch", ex.Message);
    }

    // ✅ 6. ValidateConfiguration() validates eagerly at build time
    [Fact]
    public void Should_Throw_At_Build_Time_When_ValidateConfiguration_Opted_In()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new TransitioMapperConfiguration(cfg =>
            {
                cfg.ValidateConfiguration();
                cfg.CreateMap<User, BadDto>();
            }));

        Assert.Contains("Missing source property", ex.Message);
    }

    // ✅ 7. ValidateConfiguration() with a valid config builds cleanly
    [Fact]
    public void Should_Not_Throw_At_Build_Time_For_Valid_Config_When_Opted_In()
    {
        var exception = Record.Exception(() =>
            new TransitioMapperConfiguration(cfg =>
            {
                cfg.ValidateConfiguration();
                cfg.CreateMap<User, UserDto>();
            }));

        Assert.Null(exception);
    }
}
