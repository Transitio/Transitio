using System.Collections.Generic;
using Transitio.Assertions;
 
namespace Transitio.Assertions.Tests;
 
public class DictionaryAssertionsTests
{
    [Fact]
    public void ContainKey_Passes_When_Present_Fails_When_Absent()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
 
        dict.Should().ContainKey("a");
 
        Assert.Throws<AssertionException>(() => dict.Should().ContainKey("b"));
    }
 
    [Fact]
    public void NotContainKey_Passes_When_Absent_Fails_When_Present()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
 
        dict.Should().NotContainKey("b");
 
        Assert.Throws<AssertionException>(() => dict.Should().NotContainKey("a"));
    }
 
    [Fact]
    public void ContainValue_Passes_When_Present_Fails_When_Absent()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 1 };
 
        dict.Should().ContainValue(1);
 
        Assert.Throws<AssertionException>(() => dict.Should().ContainValue(2));
    }
 
    [Fact]
    public void NotContainValue_Passes_When_Absent_Fails_When_Present()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
 
        dict.Should().NotContainValue(2);
 
        Assert.Throws<AssertionException>(() => dict.Should().NotContainValue(1));
    }
 
    [Fact]
    public void Assertions_Chain_With_And()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
 
        dict.Should().ContainKey("a").And.ContainValue(1);
    }
 
    [Fact]
    public void Should_On_Null_Dictionary_Fails_ContainKey_And_Passes_NotContainKey()
    {
        IDictionary<string, int>? dict = null;
 
        dict.Should().NotContainKey("a");
        dict.Should().NotContainValue(1);
 
        Assert.Throws<AssertionException>(() => dict.Should().ContainKey("a"));
        Assert.Throws<AssertionException>(() => dict.Should().ContainValue(1));
    }
 
    [Fact]
    public void Dictionary_Should_Resolves_To_DictionaryAssertions_Not_CollectionAssertions()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
 
        Assert.IsType<DictionaryAssertions<string, int>>(dict.Should());
    }
}