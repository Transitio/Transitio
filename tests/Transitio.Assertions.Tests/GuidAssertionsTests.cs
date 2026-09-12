using System;
using Transitio.Assertions;
 
namespace Transitio.Assertions.Tests;
 
public class GuidAssertionsTests
{
    [Fact]
    public void Be_And_NotBe()
    {
        var id = Guid.NewGuid();
        var other = Guid.NewGuid();
 
        id.Should().Be(id);
        id.Should().NotBe(other);
 
        Assert.Throws<AssertionException>(() => id.Should().Be(other));
        Assert.Throws<AssertionException>(() => id.Should().NotBe(id));
    }
 
    [Fact]
    public void BeNull_And_NotBeNull_For_Nullable_Guid()
    {
        Guid? nothing = null;
        Guid? something = Guid.NewGuid();
 
        nothing.Should().BeNull();
        something.Should().NotBeNull();
 
        Assert.Throws<AssertionException>(() => nothing.Should().NotBeNull());
        Assert.Throws<AssertionException>(() => something.Should().BeNull());
    }
 
    [Fact]
    public void Guid_Should_Resolves_To_GuidAssertions()
    {
        Assert.IsType<GuidAssertions>(Guid.NewGuid().Should());
    }
 
    [Fact]
    public void Default_And_Empty_Are_Distinct_From_A_New_Guid()
    {
        Guid.Empty.Should().Be(default(Guid));
        Guid.NewGuid().Should().NotBe(Guid.Empty);
    }
}