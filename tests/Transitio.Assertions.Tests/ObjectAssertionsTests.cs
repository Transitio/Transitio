using System;
using Transitio.Assertions;

namespace Transitio.Assertions.Tests;

public class ObjectAssertionsTests
{
    private sealed class Animal { }
    private sealed class Dog { }

    [Fact]
    public void Be_Equal_Values_Passes()
    {
        var value = new { Id = 1 };
        value.Should().Be(new { Id = 1 });
    }

    [Fact]
    public void Be_Different_Values_Throws()
    {
        var ex = Assert.Throws<AssertionException>(() => "a".Should().Be("b"));
        Assert.Contains("to be", ex.Message);
        Assert.Contains("found", ex.Message);
    }

    [Fact]
    public void NotBe_Different_Values_Passes()
    {
        object value = 1;
        value.Should().NotBe(2);
    }

    [Fact]
    public void NotBe_Equal_Values_Throws()
        => Assert.Throws<AssertionException>(() => 1.Should().NotBe(1));

    [Fact]
    public void BeNull_And_NotBeNull()
    {
        object? nothing = null;
        nothing.Should().BeNull();

        var something = new Animal();
        something.Should().NotBeNull();

        Assert.Throws<AssertionException>(() => something.Should().BeNull());
        Assert.Throws<AssertionException>(() => nothing.Should().NotBeNull());
    }

    [Fact]
    public void BeOfType_Exact_Match_Passes_And_Derived_Reported()
    {
        object dog = new Dog();
        dog.Should().BeOfType<Dog>();
        dog.Should().BeOfType(typeof(Dog));

        Assert.Throws<AssertionException>(() => dog.Should().BeOfType<Animal>());
    }

    [Fact]
    public void NotBeOfType_Passes_For_Other_Type()
    {
        object dog = new Dog();
        dog.Should().NotBeOfType<Animal>();
        Assert.Throws<AssertionException>(() => dog.Should().NotBeOfType<Dog>());
    }

    [Fact]
    public void BeAssignableTo_Interface()
    {
        object list = new System.Collections.Generic.List<int>();
        list.Should().BeAssignableTo<System.Collections.IEnumerable>();
    }

    [Fact]
    public void BeSameAs_And_NotBeSameAs()
    {
        var a = new Animal();
        var b = new Animal();
        a.Should().BeSameAs(a).And.NotBeSameAs(b);

        Assert.Throws<AssertionException>(() => a.Should().BeSameAs(b));
    }

    [Fact]
    public void Because_Reason_Is_Included_In_Message()
    {
        var ex = Assert.Throws<AssertionException>(() => "a".Should().Be("b", "the id was reused"));
        Assert.Contains("because the id was reused", ex.Message);
    }
}
