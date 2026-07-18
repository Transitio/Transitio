using System;
using System.Threading.Tasks;

namespace Transitio.Assertions;

/// <summary>
/// Assertions over an <see cref="Action"/> that verify its exception behaviour.
/// <code>
/// Action act = () =&gt; service.Withdraw(-1);
/// act.Should().Throw&lt;ArgumentException&gt;().WithMessage("*amount*");
/// </code>
/// </summary>
public class ActionAssertions
{
    private readonly Action _subject;

    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public ActionAssertions(Action subject)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }

    /// <summary>Asserts the action throws <typeparamref name="TException"/> (or a derived type).</summary>
    public ExceptionAssertions<TException> Throw<TException>(string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        try
        {
            _subject();
        }
        catch (TException ex)
        {
            return new ExceptionAssertions<TException>(ex);
        }
        catch (Exception ex)
        {
            throw new AssertionException(
                $"Expected action to throw {Formatter.Format(typeof(TException))}{AssertionReason.Build(because, becauseArgs)}, but it threw {Formatter.Format(ex.GetType())}: {ex.Message}");
        }

        throw new AssertionException(
            $"Expected action to throw {Formatter.Format(typeof(TException))}{AssertionReason.Build(because, becauseArgs)}, but no exception was thrown.");
    }

    /// <summary>Asserts the action completes without throwing.</summary>
    public void NotThrow(string because = "", params object[] becauseArgs)
    {
        try
        {
            _subject();
        }
        catch (Exception ex)
        {
            throw new AssertionException(
                $"Expected action not to throw{AssertionReason.Build(because, becauseArgs)}, but it threw {Formatter.Format(ex.GetType())}: {ex.Message}");
        }
    }
}

/// <summary>
/// Assertions over a <see cref="Func{Task}"/> that verify its asynchronous exception behaviour.
/// <code>
/// Func&lt;Task&gt; act = () =&gt; service.SaveAsync(null);
/// await act.Should().ThrowAsync&lt;ArgumentNullException&gt;();
/// </code>
/// </summary>
public class AsyncActionAssertions
{
    private readonly Func<Task> _subject;

    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public AsyncActionAssertions(Func<Task> subject)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }

    /// <summary>Asserts the awaited function throws <typeparamref name="TException"/> (or a derived type).</summary>
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because = "", params object[] becauseArgs)
        where TException : Exception
    {
        try
        {
            await _subject().ConfigureAwait(false);
        }
        catch (TException ex)
        {
            return new ExceptionAssertions<TException>(ex);
        }
        catch (Exception ex)
        {
            throw new AssertionException(
                $"Expected function to throw {Formatter.Format(typeof(TException))}{AssertionReason.Build(because, becauseArgs)}, but it threw {Formatter.Format(ex.GetType())}: {ex.Message}");
        }

        throw new AssertionException(
            $"Expected function to throw {Formatter.Format(typeof(TException))}{AssertionReason.Build(because, becauseArgs)}, but no exception was thrown.");
    }

    /// <summary>Asserts the awaited function completes without throwing.</summary>
    public async Task NotThrowAsync(string because = "", params object[] becauseArgs)
    {
        try
        {
            await _subject().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new AssertionException(
                $"Expected function not to throw{AssertionReason.Build(because, becauseArgs)}, but it threw {Formatter.Format(ex.GetType())}: {ex.Message}");
        }
    }
}

/// <summary>
/// Assertions over an exception that was captured by <see cref="ActionAssertions.Throw{TException}"/>
/// or <see cref="AsyncActionAssertions.ThrowAsync{TException}"/>.
/// </summary>
/// <typeparam name="TException">The captured exception type.</typeparam>
public class ExceptionAssertions<TException>
    where TException : Exception
{
    /// <summary>Creates the assertion wrapping the captured <paramref name="exception"/>.</summary>
    public ExceptionAssertions(TException exception)
    {
        Which = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>The captured exception, for further inspection.</summary>
    public TException Which { get; }

    /// <summary>Asserts the exception message matches a wildcard <paramref name="pattern"/> (<c>*</c>/<c>?</c>).</summary>
    public ExceptionAssertions<TException> WithMessage(string pattern, string because = "", params object[] becauseArgs)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        if (!Formatter.WildcardMatches(Which.Message, pattern))
            throw new AssertionException(
                $"Expected exception message to match {Formatter.Format(pattern)}{AssertionReason.Build(because, becauseArgs)}, but found {Formatter.Format(Which.Message)}.");

        return this;
    }

    /// <summary>Asserts the exception has an inner exception of type <typeparamref name="TInner"/> (or derived).</summary>
    public ExceptionAssertions<TInner> WithInnerException<TInner>(string because = "", params object[] becauseArgs)
        where TInner : Exception
    {
        if (Which.InnerException is not TInner inner)
            throw new AssertionException(
                $"Expected inner exception of type {Formatter.Format(typeof(TInner))}{AssertionReason.Build(because, becauseArgs)}, but found {Formatter.Format(Which.InnerException?.GetType())}.");

        return new ExceptionAssertions<TInner>(inner);
    }

    /// <summary>Asserts the captured exception satisfies <paramref name="predicate"/>.</summary>
    public ExceptionAssertions<TException> Where(Func<TException, bool> predicate, string because = "", params object[] becauseArgs)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (!predicate(Which))
            throw new AssertionException(
                $"Expected exception to satisfy the given condition{AssertionReason.Build(because, becauseArgs)}, but it did not.");

        return this;
    }
}
