namespace Transitio.Assertions;

/// <summary>
/// Assertions for <see cref="bool"/> values (including nullable booleans).
/// <code>
/// isActive.Should().BeTrue();
/// hasErrors.Should().BeFalse("the payload was valid");
/// </code>
/// </summary>
public class BooleanAssertions : AssertionsBase<bool?, BooleanAssertions>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public BooleanAssertions(bool? subject)
        : base(subject)
    {
    }

    /// <summary>Asserts the value is <c>true</c>.</summary>
    public AndConstraint<BooleanAssertions> BeTrue(string because = "", params object[] becauseArgs)
    {
        Guard(Subject == true,
            $"Expected value to be True{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value is <c>false</c>.</summary>
    public AndConstraint<BooleanAssertions> BeFalse(string because = "", params object[] becauseArgs)
    {
        Guard(Subject == false,
            $"Expected value to be False{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }

    /// <summary>Asserts the value equals <paramref name="expected"/>.</summary>
    public AndConstraint<BooleanAssertions> Be(bool expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject == expected,
            $"Expected value to be {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }
}
