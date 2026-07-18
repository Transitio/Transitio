using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class BooleanAssertionsTests
{
    [Fact]
    public void BeTrue_Passes_And_Fails()
    {
        true.Should().BeTrue();
        Assert.Throws<AssertionException>(() => false.Should().BeTrue());
    }

    [Fact]
    public void BeFalse_Passes_And_Fails()
    {
        false.Should().BeFalse();
        Assert.Throws<AssertionException>(() => true.Should().BeFalse());
    }

    [Fact]
    public void Be_Matches_Expected()
    {
        true.Should().Be(true).And.Be(true);
        Assert.Throws<AssertionException>(() => true.Should().Be(false));
    }

    [Fact]
    public void Nullable_Bool_Without_Value_Fails_BeTrue()
    {
        bool? unset = null;
        Assert.Throws<AssertionException>(() => unset.Should().BeTrue());
    }
}
