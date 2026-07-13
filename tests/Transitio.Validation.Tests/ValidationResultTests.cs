using System.Collections.Generic;

namespace Transitio.Validation.Tests;

public class ValidationResultTests
{
    [Fact]
    public void Empty_Result_Is_Valid()
    {
        var result = new ValidationResult();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Result_With_Failures_Is_Invalid()
    {
        var result = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "'Name' must not be empty."),
        });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ToString_Joins_Messages_With_Newlines()
    {
        var result = new ValidationResult(new List<ValidationFailure>
        {
            new("Name", "Name is bad"),
            new("Age", "Age is bad"),
        });

        Assert.Equal("Name is bad\nAge is bad", result.ToString());
    }

    [Fact]
    public void Failure_Captures_AttemptedValue_And_ErrorCode()
    {
        var failure = new ValidationFailure("Age", "too small", attemptedValue: -1, errorCode: "GreaterThan");

        Assert.Equal("Age", failure.PropertyName);
        Assert.Equal(-1, failure.AttemptedValue);
        Assert.Equal("GreaterThan", failure.ErrorCode);
    }
}
