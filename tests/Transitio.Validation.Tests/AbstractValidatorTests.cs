using System;
using System.Linq;

namespace Transitio.Validation.Tests;

public class AbstractValidatorTests
{
    [Fact]
    public void Valid_Instance_Passes()
    {
        var validator = new CustomerValidator();

        var result = validator.Validate(new Customer
        {
            Name = "Ada Lovelace",
            Email = "ada@example.com",
            Age = 36,
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Failure_PropertyName_Comes_From_Expression()
    {
        var validator = new CustomerValidator();

        var result = validator.Validate(new Customer { Name = "", Email = "ada@example.com", Age = 1 });

        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Continue_Cascade_Reports_All_Failures()
    {
        var validator = new CustomerValidator();

        // Name empty, Email invalid, Age out of range -> at least three failures.
        var result = validator.Validate(new Customer { Name = "", Email = "nope", Age = 999 });

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3, $"Expected >= 3 failures, got {result.Errors.Count}");
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Age");
    }

    [Fact]
    public void ValidateAndThrow_Throws_On_Invalid()
    {
        var validator = new CustomerValidator();

        var ex = Assert.Throws<ValidationException>(() =>
            validator.ValidateAndThrow(new Customer { Name = "", Email = "x", Age = -1 }));

        Assert.NotEmpty(ex.Errors);
        Assert.False(ex.Result.IsValid);
    }

    [Fact]
    public void ValidateAndThrow_Does_Not_Throw_On_Valid()
    {
        var validator = new CustomerValidator();

        var exception = Record.Exception(() =>
            validator.ValidateAndThrow(new Customer { Name = "Grace", Email = "grace@example.com", Age = 40 }));

        Assert.Null(exception);
    }

    [Fact]
    public void NonGeneric_Validate_Casts_And_Validates()
    {
        IValidator validator = new CustomerValidator();

        var result = validator.Validate((object)new Customer { Name = "", Email = "x", Age = -1 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void NonGeneric_Validate_Throws_On_Wrong_Type()
    {
        IValidator validator = new CustomerValidator();

        Assert.Throws<ArgumentException>(() => validator.Validate("not a customer"));
    }

    [Fact]
    public void Validate_Throws_On_Null_Instance()
    {
        var validator = new CustomerValidator();

        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public void RuleFor_Rejects_Non_Member_Expression()
    {
        Assert.Throws<ArgumentException>(() =>
            new ConfigurableValidator(v => v.For(c => c.Name + "!")));
    }
}
