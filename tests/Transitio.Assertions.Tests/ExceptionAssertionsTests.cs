using System;
using System.Threading.Tasks;
using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class ExceptionAssertionsTests
{
    [Fact]
    public void Throw_Captures_Expected_Exception()
    {
        Action act = () => throw new InvalidOperationException("boom");
        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Be("boom");
    }

    [Fact]
    public void Throw_Wrong_Type_Fails()
    {
        Action act = () => throw new InvalidOperationException("boom");
        Assert.Throws<AssertionException>(() => act.Should().Throw<ArgumentException>());
    }

    [Fact]
    public void Throw_When_Nothing_Thrown_Fails()
    {
        Action act = () => { };
        var ex = Assert.Throws<AssertionException>(() => act.Should().Throw<Exception>());
        Assert.Contains("no exception was thrown", ex.Message);
    }

    [Fact]
    public void WithMessage_Wildcard()
    {
        Action act = () => throw new ArgumentException("amount must be positive");
        act.Should().Throw<ArgumentException>().WithMessage("*amount*");

        Action act2 = () => throw new ArgumentException("nope");
        Assert.Throws<AssertionException>(() => act2.Should().Throw<ArgumentException>().WithMessage("*amount*"));
    }

    [Fact]
    public void WithInnerException()
    {
        Action act = () => throw new InvalidOperationException("outer", new FormatException("inner"));
        act.Should().Throw<InvalidOperationException>()
            .WithInnerException<FormatException>()
            .Which.Message.Should().Be("inner");
    }

    [Fact]
    public void Where_Predicate()
    {
        Action act = () => throw new ArgumentException("x", "paramName");
        act.Should().Throw<ArgumentException>().Where(e => e.ParamName == "paramName");

        Assert.Throws<AssertionException>(
            () => act.Should().Throw<ArgumentException>().Where(e => e.ParamName == "other"));
    }

    [Fact]
    public void NotThrow_Passes_And_Fails()
    {
        Action ok = () => { };
        ok.Should().NotThrow();

        Action bad = () => throw new Exception("x");
        Assert.Throws<AssertionException>(() => bad.Should().NotThrow());
    }

    [Fact]
    public async Task ThrowAsync_Captures_Expected_Exception()
    {
        Func<Task> act = () => throw new InvalidOperationException("async boom");
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("async boom");
    }

    [Fact]
    public async Task NotThrowAsync_Passes()
    {
        Func<Task> ok = () => Task.CompletedTask;
        await ok.Should().NotThrowAsync();
    }
}
