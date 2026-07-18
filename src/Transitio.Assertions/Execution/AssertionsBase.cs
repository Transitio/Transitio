namespace Transitio.Assertions;

/// <summary>
/// Base class for all assertion types. Holds the subject under test, produces the
/// <see cref="AndConstraint{T}"/> continuation, and centralises failure reporting so every
/// assertion throws a consistent <see cref="AssertionException"/>.
/// </summary>
/// <typeparam name="TSubject">The type of the value being asserted on.</typeparam>
/// <typeparam name="TAssertions">The concrete assertion type (self-referential, for <c>.And</c> chaining).</typeparam>
public abstract class AssertionsBase<TSubject, TAssertions>
    where TAssertions : AssertionsBase<TSubject, TAssertions>
{
    /// <summary>Creates the assertion for the given <paramref name="subject"/>.</summary>
    protected AssertionsBase(TSubject? subject)
    {
        Subject = subject;
    }

    /// <summary>The value under test.</summary>
    public TSubject? Subject { get; }

    /// <summary>Continuation that lets the caller chain further assertions with <c>.And</c>.</summary>
    protected AndConstraint<TAssertions> Continue => new((TAssertions)this);

    /// <summary>Throws an <see cref="AssertionException"/> when <paramref name="condition"/> is false.</summary>
    protected void Guard(bool condition, string message)
    {
        if (!condition)
            throw new AssertionException(message);
    }

    /// <summary>Always throws an <see cref="AssertionException"/> with the given message.</summary>
    protected static AssertionException Failure(string message) => new(message);

    /// <summary>
    /// Turns the optional <c>because</c> reason into a message fragment such as
    /// <c>" because the id was reused"</c> (leading space included), or an empty string when no
    /// reason was supplied. A "because" prefix is added when the caller omits it.
    /// </summary>
    protected static string Reason(string because, object[] becauseArgs)
        => AssertionReason.Build(because, becauseArgs);
}
