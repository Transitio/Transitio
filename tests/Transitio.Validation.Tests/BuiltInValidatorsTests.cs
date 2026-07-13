using System.Linq;

namespace Transitio.Validation.Tests;

public class BuiltInValidatorsTests
{
    // Each fact configures one rule in isolation via the shared ConfigurableValidator helper.
    private static ValidationResult Run(
        Customer customer,
        System.Action<ConfigurableValidator> configure)
        => new ConfigurableValidator(configure).Validate(customer);

    [Fact]
    public void NotNull_Fails_On_Null_Passes_On_Value()
    {
        Assert.False(Run(new Customer { Name = null }, v => v.For(c => c.Name).NotNull()).IsValid);
        Assert.True(Run(new Customer { Name = "x" }, v => v.For(c => c.Name).NotNull()).IsValid);
    }

    [Fact]
    public void NotEmpty_Treats_Whitespace_String_As_Empty()
    {
        Assert.False(Run(new Customer { Name = "   " }, v => v.For(c => c.Name).NotEmpty()).IsValid);
        Assert.True(Run(new Customer { Name = "x" }, v => v.For(c => c.Name).NotEmpty()).IsValid);
    }

    [Fact]
    public void NotEmpty_Detects_Empty_Collection()
    {
        Assert.False(Run(new Customer { Tags = new() }, v => v.For(c => c.Tags).NotEmpty()).IsValid);
        Assert.True(Run(new Customer { Tags = new() { "a" } }, v => v.For(c => c.Tags).NotEmpty()).IsValid);
    }

    [Fact]
    public void Must_Uses_Custom_Predicate()
    {
        Assert.False(Run(new Customer { Age = 3 }, v => v.For(c => c.Age).Must(a => a % 2 == 0)).IsValid);
        Assert.True(Run(new Customer { Age = 4 }, v => v.For(c => c.Age).Must(a => a % 2 == 0)).IsValid);
    }

    [Fact]
    public void Equal_And_NotEqual()
    {
        Assert.True(Run(new Customer { Age = 5 }, v => v.For(c => c.Age).Equal(5)).IsValid);
        Assert.False(Run(new Customer { Age = 6 }, v => v.For(c => c.Age).Equal(5)).IsValid);
        Assert.True(Run(new Customer { Age = 6 }, v => v.For(c => c.Age).NotEqual(5)).IsValid);
    }

    [Fact]
    public void Length_Bounds()
    {
        Assert.True(Run(new Customer { Name = "abc" }, v => v.For(c => c.Name).Length(1, 3)).IsValid);
        Assert.False(Run(new Customer { Name = "abcd" }, v => v.For(c => c.Name).Length(1, 3)).IsValid);
    }

    [Fact]
    public void Minimum_And_Maximum_Length()
    {
        Assert.False(Run(new Customer { Name = "a" }, v => v.For(c => c.Name).MinimumLength(2)).IsValid);
        Assert.False(Run(new Customer { Name = "abc" }, v => v.For(c => c.Name).MaximumLength(2)).IsValid);
    }

    [Fact]
    public void Matches_Regex()
    {
        Assert.True(Run(new Customer { Name = "A1" }, v => v.For(c => c.Name).Matches("^[A-Z][0-9]$")).IsValid);
        Assert.False(Run(new Customer { Name = "11" }, v => v.For(c => c.Name).Matches("^[A-Z][0-9]$")).IsValid);
    }

    [Theory]
    [InlineData("ada@example.com", true)]
    [InlineData("not-an-email", false)]
    public void EmailAddress(string email, bool expectedValid)
        => Assert.Equal(expectedValid,
            Run(new Customer { Email = email }, v => v.For(c => c.Email).EmailAddress()).IsValid);

    [Fact]
    public void Comparison_Rules()
    {
        Assert.True(Run(new Customer { Age = 5 }, v => v.For(c => c.Age).GreaterThan(4)).IsValid);
        Assert.False(Run(new Customer { Age = 4 }, v => v.For(c => c.Age).GreaterThan(4)).IsValid);
        Assert.True(Run(new Customer { Age = 4 }, v => v.For(c => c.Age).GreaterThanOrEqual(4)).IsValid);
        Assert.True(Run(new Customer { Age = 3 }, v => v.For(c => c.Age).LessThan(4)).IsValid);
        Assert.True(Run(new Customer { Age = 4 }, v => v.For(c => c.Age).LessThanOrEqual(4)).IsValid);
        Assert.True(Run(new Customer { Age = 5 }, v => v.For(c => c.Age).InclusiveBetween(1, 10)).IsValid);
        Assert.False(Run(new Customer { Age = 11 }, v => v.For(c => c.Age).InclusiveBetween(1, 10)).IsValid);
    }

    [Fact]
    public void Null_Value_Skips_Value_Rules()
    {
        // Length/Email on a null string should not produce a failure on its own.
        Assert.True(Run(new Customer { Name = null }, v => v.For(c => c.Name).MaximumLength(3)).IsValid);
        Assert.True(Run(new Customer { Email = null }, v => v.For(c => c.Email).EmailAddress()).IsValid);
    }

    [Fact]
    public void WithMessage_Overrides_Default_Message()
    {
        var result = Run(new Customer { Name = "" }, v => v.For(c => c.Name).NotEmpty().WithMessage("Name is required"));

        Assert.Equal("Name is required", result.Errors.Single().ErrorMessage);
    }

    [Fact]
    public void WithErrorCode_Overrides_Default_Code()
    {
        var result = Run(new Customer { Name = "" }, v => v.For(c => c.Name).NotEmpty().WithErrorCode("REQUIRED"));

        Assert.Equal("REQUIRED", result.Errors.Single().ErrorCode);
    }
}
