namespace Transitio.Assertions;
 
/// <summary>
/// Assertions for a <see cref="Guid"/> (equality and nullability only — no ordering, since Guid
/// ordering isn't meaningful, and no reference-identity checks, since a boxed Guid's reference
/// never reflects value equality).
/// <code>
/// id.Should().NotBe(Guid.Empty);
/// correlationId.Should().Be(expectedId);
/// </code>
/// </summary>
public class GuidAssertions : AssertionsBase<Guid?, GuidAssertions>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public GuidAssertions(Guid? subject)
        : base(subject)
    {
    }
 
    /// <summary>Asserts the subject equals <paramref name="expected"/>.</summary>
    public AndConstraint<GuidAssertions> Be(Guid expected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject == expected,
            $"Expected value to be {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }
 
    /// <summary>Asserts the subject does not equal <paramref name="unexpected"/>.</summary>
    public AndConstraint<GuidAssertions> NotBe(Guid unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(Subject != unexpected,
            $"Expected value not to be {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }
 
    /// <summary>Asserts the subject is <c>null</c>.</summary>
    public AndConstraint<GuidAssertions> BeNull(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is null,
            $"Expected value to be <null>{Reason(because, becauseArgs)}, but found {Formatter.Format(Subject)}.");
        return Continue;
    }
 
    /// <summary>Asserts the subject is not <c>null</c>.</summary>
    public AndConstraint<GuidAssertions> NotBeNull(string because = "", params object[] becauseArgs)
    {
        Guard(Subject is not null,
            $"Expected value not to be <null>{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }
}