using System.Collections.Generic;
using System.Linq;
using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class CollectionAssertionsTests
{
    [Fact]
    public void Empty_Variants()
    {
        new int[0].Should().BeEmpty();
        new[] { 1 }.Should().NotBeEmpty();

        Assert.Throws<AssertionException>(() => new[] { 1 }.Should().BeEmpty());
        Assert.Throws<AssertionException>(() => new int[0].Should().NotBeEmpty());
    }

    [Fact]
    public void Counts()
    {
        new[] { 1, 2, 3 }.Should().HaveCount(3)
            .And.HaveCountGreaterThan(2)
            .And.HaveCountLessThan(4);

        Assert.Throws<AssertionException>(() => new[] { 1, 2 }.Should().HaveCount(3));
    }

    [Fact]
    public void Contain_Item_And_Predicate()
    {
        var names = new[] { "Ada", "Alan" };
        names.Should().Contain("Ada").And.Contain(n => n.StartsWith("Al"));
        names.Should().NotContain("Grace");

        Assert.Throws<AssertionException>(() => names.Should().Contain("Grace"));
        Assert.Throws<AssertionException>(() => names.Should().Contain(n => n.Length > 10));
    }

    [Fact]
    public void ContainSingle()
    {
        new[] { 42 }.Should().ContainSingle();
        Assert.Throws<AssertionException>(() => new[] { 1, 2 }.Should().ContainSingle());
    }

    [Fact]
    public void OnlyContain()
    {
        new[] { 2, 4, 6 }.Should().OnlyContain(n => n % 2 == 0);
        Assert.Throws<AssertionException>(() => new[] { 2, 3 }.Should().OnlyContain(n => n % 2 == 0));
    }

    [Fact]
    public void OnlyContain_Fails_On_Empty_Collection()
        => Assert.Throws<AssertionException>(() => new int[0].Should().OnlyContain(n => n > 0));

    [Fact]
    public void BeEquivalentTo_Ignores_Order()
    {
        new[] { 1, 2, 3 }.Should().BeEquivalentTo(new[] { 3, 2, 1 });

        Assert.Throws<AssertionException>(() => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 1, 2, 3 }));
        Assert.Throws<AssertionException>(() => new[] { 1, 1 }.Should().BeEquivalentTo(new[] { 1, 2 }));
    }

    [Fact]
    public void Materializes_Lazy_Sequence_Once()
    {
        var enumerationCount = 0;
        IEnumerable<int> Sequence()
        {
            enumerationCount++;
            yield return 1;
            yield return 2;
        }

        Sequence().Should().HaveCount(2).And.Contain(1).And.Contain(2);
        Assert.Equal(1, enumerationCount);
    }
}
