using System;
using System.Numerics;

namespace Transitio.Assertions;

/// <summary>
/// Assertions for numeric value types. Adds sign and tolerance checks on top of the ordering and
/// range assertions inherited from <see cref="ComparableAssertionsBase{T, TAssertions}"/>.
/// <code>
/// age.Should().BeGreaterThan(0).And.BePositive();
/// total.Should().BeInRange(1m, 100m);
/// (0.1 + 0.2).Should().BeApproximately(0.3, 1e-9);
/// </code>
/// </summary>
/// <typeparam name="T">A numeric value type (implements <see cref="INumber{T}"/>).</typeparam>
public sealed class NumericAssertions<T> : ComparableAssertionsBase<T, NumericAssertions<T>>
    where T : struct, INumber<T>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public NumericAssertions(T? subject)
        : base(subject)
    {
    }

    /// <summary>Asserts the value is greater than zero.</summary>
    public AndConstraint<NumericAssertions<T>> BePositive(string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(T.Zero) > 0,
            $"Expected value to be positive{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value is less than zero.</summary>
    public AndConstraint<NumericAssertions<T>> BeNegative(string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && Subject.Value.CompareTo(T.Zero) < 0,
            $"Expected value to be negative{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>
    /// Asserts the value is within <paramref name="tolerance"/> of <paramref name="expected"/>
    /// (i.e. <c>|value - expected| &lt;= tolerance</c>) - the safe way to compare floating-point numbers.
    /// </summary>
    public AndConstraint<NumericAssertions<T>> BeApproximately(T expected, T tolerance, string because = "", params object[] becauseArgs)
    {
        Guard(Subject.HasValue && T.Abs(Subject.Value - expected).CompareTo(T.Abs(tolerance)) <= 0,
            $"Expected value to be approximately {Formatter.Format(expected)} (\u00b1{Formatter.Format(tolerance)}){Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }
}
