using System;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Dependency.Tests;

/// <summary>
/// Verifies that opting in via cfg.ValidateConfiguration() makes AddTransitio fail fast
/// at registration time, rather than deferring the error to the first Map call.
/// </summary>
public class StartupValidationTests
{
    // Destination has a property (Email) with no matching source property on User.
    public class UserWithEmailDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
    }

    [Fact]
    public void Should_Throw_At_AddTransitio_When_Validation_Opted_In_And_Config_Invalid()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddTransitio(cfg =>
            {
                cfg.ValidateConfiguration();
                cfg.CreateMap<User, UserWithEmailDto>();
            }));

        Assert.Contains("Missing source property", ex.Message);
    }

    [Fact]
    public void Should_Not_Throw_At_AddTransitio_For_Valid_Config_When_Opted_In()
    {
        var services = new ServiceCollection();

        var exception = Record.Exception(() =>
            services.AddTransitio(cfg =>
            {
                cfg.ValidateConfiguration();
                cfg.CreateMap<User, UserDto>();
            }));

        Assert.Null(exception);
    }

    [Fact]
    public void Should_Not_Validate_When_Not_Opted_In()
    {
        var services = new ServiceCollection();

        // Invalid map, but no ValidateConfiguration() call - registration must succeed.
        var exception = Record.Exception(() =>
            services.AddTransitio(cfg =>
            {
                cfg.CreateMap<User, UserWithEmailDto>();
            }));

        Assert.Null(exception);
    }
}
