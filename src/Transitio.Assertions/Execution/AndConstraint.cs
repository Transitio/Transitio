namespace Transitio.Assertions;

/// <summary>
/// Returned by every assertion so results can be chained fluently:
/// <code>
/// value.Should().BeGreaterThan(0).And.BeLessThan(10);
/// </code>
/// </summary>
/// <typeparam name="T">The assertion type the chain continues on.</typeparam>
public sealed class AndConstraint<T>
{
    /// <summary>Creates a continuation wrapping the assertion instance.</summary>
    public AndConstraint(T parent)
    {
        And = parent;
    }

    /// <summary>The assertion instance, so further assertions can be applied to the same subject.</summary>
    public T And { get; }
}
