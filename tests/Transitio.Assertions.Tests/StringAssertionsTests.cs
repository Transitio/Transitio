using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class StringAssertionsTests
{
    [Fact]
    public void Be_And_NotBe()
    {
        "abc".Should().Be("abc").And.NotBe("xyz");
        Assert.Throws<AssertionException>(() => "abc".Should().Be("ABC"));
    }

    [Fact]
    public void Empty_Variants()
    {
        "".Should().BeEmpty();
        "x".Should().NotBeEmpty();
        ((string?)null).Should().BeNullOrEmpty();
        "".Should().BeNullOrEmpty();
        " ".Should().BeNullOrWhiteSpace();
        "x".Should().NotBeNullOrWhiteSpace();

        Assert.Throws<AssertionException>(() => "x".Should().BeEmpty());
        Assert.Throws<AssertionException>(() => "x".Should().BeNullOrWhiteSpace());
    }

    [Fact]
    public void NotBeEmpty_Fails_On_Null_And_Empty()
    {
        Assert.Throws<AssertionException>(() => ((string?)null).Should().NotBeEmpty());
        Assert.Throws<AssertionException>(() => "".Should().NotBeEmpty());
    }

    [Fact]
    public void Contain_StartWith_EndWith()
    {
        "hello world".Should().Contain("o w").And.StartWith("hello").And.EndWith("world");
        "hello".Should().NotContain("z");

        Assert.Throws<AssertionException>(() => "hello".Should().Contain("z"));
        Assert.Throws<AssertionException>(() => "hello".Should().StartWith("ello"));
        Assert.Throws<AssertionException>(() => "hello".Should().EndWith("hell"));
    }

    [Fact]
    public void HaveLength()
    {
        "hello".Should().HaveLength(5);
        Assert.Throws<AssertionException>(() => "hello".Should().HaveLength(4));
    }

    [Theory]
    [InlineData("order-123", "order-*", true)]
    [InlineData("order-123", "*-123", true)]
    [InlineData("order-123", "order-???", true)]
    [InlineData("order-123", "order-??", false)]
    [InlineData("abc", "a?c", true)]
    [InlineData("abc", "a?d", false)]
    [InlineData("order-2026-07-15", "order-*-*-*", true)]
    [InlineData("aaaaaaaaaab", "*a*a*b", true)]
    [InlineData("aaaaaaaaaa", "*a*a*b", false)]
    [InlineData("anything", "*", true)]
    [InlineData("", "*", true)]
    [InlineData("", "?", false)]
    public void Match_Wildcard(string value, string pattern, bool expected)
    {
        if (expected)
            value.Should().Match(pattern);
        else
            Assert.Throws<AssertionException>(() => value.Should().Match(pattern));
    }

    [Fact]
    public void Failure_Message_Includes_Both_Values()
    {
        var ex = Assert.Throws<AssertionException>(() => "abc".Should().Be("xyz"));
        Assert.Contains("\"xyz\"", ex.Message);
        Assert.Contains("\"abc\"", ex.Message);
    }
}
