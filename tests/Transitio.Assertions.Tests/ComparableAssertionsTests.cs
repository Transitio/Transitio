using System;
using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class ComparableAssertionsTests
{
    [Fact]
    public void Be_And_NotBe()
    {
        5.Should().Be(5).And.NotBe(6);
        Assert.Throws<AssertionException>(() => 5.Should().Be(6));
    }

    [Fact]
    public void Ordering_Passes()
    {
        10.Should().BeGreaterThan(0)
            .And.BeGreaterThanOrEqualTo(10)
            .And.BeLessThan(11)
            .And.BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void Ordering_Fails()
    {
        Assert.Throws<AssertionException>(() => 10.Should().BeGreaterThan(10));
        Assert.Throws<AssertionException>(() => 10.Should().BeLessThan(10));
    }

    [Fact]
    public void BeInRange()
    {
        5.Should().BeInRange(1, 10);
        Assert.Throws<AssertionException>(() => 11.Should().BeInRange(1, 10));
    }

    [Fact]
    public void Positive_And_Negative()
    {
        3.Should().BePositive();
        (-3).Should().BeNegative();
        Assert.Throws<AssertionException>(() => 0.Should().BePositive());
        Assert.Throws<AssertionException>(() => 0.Should().BeNegative());
    }

    [Fact]
    public void Works_For_Decimal_And_DateTime()
    {
        9.99m.Should().BeGreaterThan(0m).And.BeLessThan(10m);

        var now = new DateTime(2026, 7, 15);
        now.Should().BeGreaterThan(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void Nullable_Without_Value_Fails()
    {
        int? unset = null;
        Assert.Throws<AssertionException>(() => unset.Should().Be(0));
    }

    [Fact]
    public void BeApproximately_Handles_Floating_Point()
    {
        (0.1 + 0.2).Should().BeApproximately(0.3, 1e-9);
        3.14f.Should().BeApproximately(3.1f, 0.05f);
        Assert.Throws<AssertionException>(() => 1.0.Should().BeApproximately(2.0, 0.5));
    }

    [Fact]
    public void And_Chaining_Keeps_Numeric_Methods_Reachable()
    {
        // After an inherited ordering assertion, .And must still expose numeric-only methods.
        10.Should().BeGreaterThan(0).And.BePositive().And.BeInRange(1, 100);
    }

    [Fact]
    public void Works_For_Additional_Numeric_Types()
    {
        ((byte)5).Should().BePositive().And.BeInRange((byte)0, (byte)10);
        5u.Should().BeGreaterThan(0u);
        5ul.Should().BeLessThan(10ul);
        ((sbyte)-2).Should().BeNegative();
    }

    [Fact]
    public void Works_For_Char_And_DateTimeOffset()
    {
        'c'.Should().BeGreaterThan('a').And.BeInRange('a', 'z');

        var when = new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero);
        when.Should().BeGreaterThan(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
