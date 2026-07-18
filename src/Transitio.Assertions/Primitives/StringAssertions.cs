using System;

namespace Transitio.Assertions;

/// <summary>
/// Assertions for <see cref="string"/> values.
/// <code>
/// name.Should().StartWith("Tra").And.EndWith("io");
/// message.Should().Contain("saved").And.HaveLength(12);
/// slug.Should().Match("order-*");
/// </code>
/// </summary>
public class StringAssertions : AssertionsBase<string?, StringAssertions>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public StringAssertions(string? subject)
        : base(subject)
    {
    }

    /// <summary>Asserts the string equals <paramref name="expected"/> (ordinal comparison).</summary>
    public AndConstraint<StringAssertions> Be(string? expected, string because = "", params object[] becauseArgs)
    {
        Guard(string.Equals(Subject, expected, StringComparison.Ordinal),
            $"Expected string to be {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string does not equal <paramref name="unexpected"/> (ordinal comparison).</summary>
    public AndConstraint<StringAssertions> NotBe(string? unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(!string.Equals(Subject, unexpected, StringComparison.Ordinal),
            $"Expected string not to be {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the string is empty (length zero).</summary>
    public AndConstraint<StringAssertions> BeEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is { Length: 0 },
            $"Expected string to be empty{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string is non-null and has at least one character.</summary>
    public AndConstraint<StringAssertions> NotBeEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is { Length: > 0 },
            $"Expected string not to be empty{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string is <c>null</c> or empty.</summary>
    public AndConstraint<StringAssertions> BeNullOrEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(string.IsNullOrEmpty(Subject),
            $"Expected string to be null or empty{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string is neither <c>null</c> nor empty.</summary>
    public AndConstraint<StringAssertions> NotBeNullOrEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(!string.IsNullOrEmpty(Subject),
            $"Expected string not to be null or empty{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string is <c>null</c>, empty, or only white-space.</summary>
    public AndConstraint<StringAssertions> BeNullOrWhiteSpace(string because = "", params object[] becauseArgs)
    {
        Guard(string.IsNullOrWhiteSpace(Subject),
            $"Expected string to be null or white-space{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string contains non-white-space characters.</summary>
    public AndConstraint<StringAssertions> NotBeNullOrWhiteSpace(string because = "", params object[] becauseArgs)
    {
        Guard(!string.IsNullOrWhiteSpace(Subject),
            $"Expected string not to be null or white-space{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the string contains <paramref name="expected"/> as a substring (ordinal).</summary>
    public AndConstraint<StringAssertions> Contain(string expected, string because = "", params object[] becauseArgs)
    {
        if (expected == null)
            throw new ArgumentNullException(nameof(expected));

        Guard(Subject is not null && Subject.Contains(expected, StringComparison.Ordinal),
            $"Expected string {Formatter.Format(Subject)} to contain {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>Asserts the string does not contain <paramref name="unexpected"/> as a substring (ordinal).</summary>
    public AndConstraint<StringAssertions> NotContain(string unexpected, string because = "", params object[] becauseArgs)
    {
        if (unexpected == null)
            throw new ArgumentNullException(nameof(unexpected));

        Guard(Subject is null || !Subject.Contains(unexpected, StringComparison.Ordinal),
            $"Expected string {Formatter.Format(Subject)} not to contain {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it did.");
        return Continue;
    }

    /// <summary>Asserts the string starts with <paramref name="expected"/> (ordinal).</summary>
    public AndConstraint<StringAssertions> StartWith(string expected, string because = "", params object[] becauseArgs)
    {
        if (expected == null)
            throw new ArgumentNullException(nameof(expected));

        Guard(Subject is not null && Subject.StartsWith(expected, StringComparison.Ordinal),
            $"Expected string {Formatter.Format(Subject)} to start with {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>Asserts the string ends with <paramref name="expected"/> (ordinal).</summary>
    public AndConstraint<StringAssertions> EndWith(string expected, string because = "", params object[] becauseArgs)
    {
        if (expected == null)
            throw new ArgumentNullException(nameof(expected));

        Guard(Subject is not null && Subject.EndsWith(expected, StringComparison.Ordinal),
            $"Expected string {Formatter.Format(Subject)} to end with {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>Asserts the string has exactly <paramref name="expected"/> characters.</summary>
    public AndConstraint<StringAssertions> HaveLength(int expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject is not null && Subject.Length == expected,
            $"Expected string to have length {expected}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject?.Length)} for {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>
    /// Asserts the string matches a wildcard <paramref name="pattern"/> where <c>*</c> matches any
    /// run of characters and <c>?</c> matches a single character.
    /// </summary>
    public AndConstraint<StringAssertions> Match(string pattern, string because = "", params object[] becauseArgs)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        Guard(Formatter.WildcardMatches(Subject, pattern),
            $"Expected string {Formatter.Format(Subject)} to match {Formatter.Format(pattern)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }
}
