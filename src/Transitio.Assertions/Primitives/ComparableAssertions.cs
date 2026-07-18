using System;

namespace Transitio.Assertions;

/// <summary>
/// Shared ordering and range assertions for comparable value types. The self-referential
/// <typeparamref name="TAssertions"/> lets derived types (e.g. <see cref="NumericAssertions{T}"/>)
/// keep their own methods reachable through <c>.And</c> chaining.
/// </summary>
/// <typeparam name="T">A comparable value type.</typeparam>
/// <typeparam name="TAssertions">The concrete assertion type.</typeparam>
public abstract class ComparableAssertionsBase<T, TAssertions> : AssertionsBase<T?, TAssertions>
    where T : struct, IComparable<T>
    where TAssertions : ComparableAssertionsBase<T, TAssertions>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    protected ComparableAssertionsBase(T? subject)
        : base(subject)
    {
    }

    /// <summary>Asserts the value equals <paramref name="expected"/>.</summary>
    public AndConstraint<TAssertions> Be(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(expected) == 0,
            $"Expected value to be {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value does not equal <paramref name="unexpected"/>.</summary>
    public AndConstraint<TAssertions> NotBe(T unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(!Subject.HasValue || Subject.Value.CompareTo(unexpected) != 0,
            $"Expected value not to be {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the value is strictly greater than <paramref name="expected"/>.</summary>
    public AndConstraint<TAssertions> BeGreaterThan(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(expected) > 0,
            $"Expected value to be greater than {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value is greater than or equal to <paramref name="expected"/>.</summary>
    public AndConstraint<TAssertions> BeGreaterThanOrEqualTo(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(expected) >= 0,
            $"Expected value to be greater than or equal to {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value is strictly less than <paramref name="expected"/>.</summary>
    public AndConstraint<TAssertions> BeLessThan(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(expected) < 0,
            $"Expected value to be less than {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value is less than or equal to <paramref name="expected"/>.</summary>
    public AndConstraint<TAssertions> BeLessThanOrEqualTo(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(expected) <= 0,
            $"Expected value to be less than or equal to {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value lies within the inclusive range [<paramref name="minimum"/>, <paramref name="maximum"/>].</summary>
    public AndConstraint<TAssertions> BeInRange(T minimum, T maximum, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(minimum) >= 0 && Subject.Value.CompareTo(maximum) <= 0,
            $"Expected value to be in range [{Formatter.Format(minimum)}, {Formatter.Format(maximum)}]{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }
}

/// <summary>
/// Assertions for comparable value types that are not numbers - <see cref="DateTime"/>,
/// <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/>, <see cref="char"/>, etc.
/// <code>
/// createdAt.Should().BeGreaterThan(new DateTime(2026, 1, 1));
/// window.Should().BeInRange(TimeSpan.Zero, TimeSpan.FromHours(1));
/// </code>
/// </summary>
/// <typeparam name="T">A comparable value type.</typeparam>
public sealed class ComparableAssertions<T> : ComparableAssertionsBase<T, ComparableAssertions<T>>
    where T : struct, IComparable<T>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public ComparableAssertions(T? subject)
        : base(subject)
    {
    }
}
