using System;

namespace Transitio.Assertions;

/// <summary>
/// Assertions for any object or value, covering equality, nullability, reference identity and type.
/// <code>
/// customer.Should().NotBeNull();
/// result.Should().Be(expected).And.BeOfType&lt;Order&gt;();
/// </code>
/// </summary>
public class ObjectAssertions : AssertionsBase<object?, ObjectAssertions>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public ObjectAssertions(object? subject)
        : base(subject)
    {
    }

    /// <summary>Asserts the subject equals <paramref name="expected"/> (using <see cref="object.Equals(object, object)"/>).</summary>
    public AndConstraint<ObjectAssertions> Be(object? expected, string because = "", params object[] becauseArgs)
    {
        Guard(Equals(Subject, expected),
            $"Expected value to be {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the subject does not equal <paramref name="unexpected"/>.</summary>
    public AndConstraint<ObjectAssertions> NotBe(object? unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(!Equals(Subject, unexpected),
            $"Expected value not to be {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the subject is <c>null</c>.</summary>
    public AndConstraint<ObjectAssertions> BeNull(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is null,
            $"Expected value to be <null>{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the subject is not <c>null</c>.</summary>
    public AndConstraint<ObjectAssertions> NotBeNull(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is not null,
            $"Expected value not to be <null>{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the subject is exactly of type <typeparamref name="TExpected"/> (not a derived type).</summary>
    public AndConstraint<ObjectAssertions> BeOfType<TExpected>(string because = "", params object[] becauseArgs)
        => BeOfType(typeof(TExpected), because, becauseArgs);

    /// <summary>Asserts the subject is exactly of type <paramref name="expectedType"/> (not a derived type).</summary>
    public AndConstraint<ObjectAssertions> BeOfType(Type expectedType, string because = "", params object[] becauseArgs)
    {
        if (expectedType == null)
            throw new ArgumentNullException(nameof(expectedType));

        Guard(Subject is not null && Subject.GetType() == expectedType,
            $"Expected value to be of type {Formatter.Format(expectedType)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject?.GetType())}.");
        return Continue;
    }

    /// <summary>Asserts the subject is not of type <typeparamref name="TUnexpected"/>.</summary>
    public AndConstraint<ObjectAssertions> NotBeOfType<TUnexpected>(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is null || Subject.GetType() != typeof(TUnexpected),
            $"Expected value not to be of type {Formatter.Format(typeof(TUnexpected))}{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the subject can be assigned to a variable of type <typeparamref name="TExpected"/>.</summary>
    public AndConstraint<ObjectAssertions> BeAssignableTo<TExpected>(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is TExpected,
            $"Expected value to be assignable to {Formatter.Format(typeof(TExpected))}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject?.GetType())}.");
        return Continue;
    }

    /// <summary>Asserts the subject is the same reference as <paramref name="expected"/>.</summary>
    public AndConstraint<ObjectAssertions> BeSameAs(object? expected, string because = "", params object[] becauseArgs)
    {
        Guard(ReferenceEquals(Subject, expected),
            $"Expected value to refer to the same instance as {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>Asserts the subject is a different reference from <paramref name="unexpected"/>.</summary>
    public AndConstraint<ObjectAssertions> NotBeSameAs(object? unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(!ReferenceEquals(Subject, unexpected),
            $"Expected value not to refer to the same instance as {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it did.");
        return Continue;
    }
}
