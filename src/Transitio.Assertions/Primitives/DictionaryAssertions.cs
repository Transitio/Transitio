using System.Collections.Generic;
 
namespace Transitio.Assertions;
 
/// <summary>
/// Assertions for a dictionary (<see cref="IDictionary{TKey,TValue}"/>) — key/value membership.
/// <code>
/// scores.Should().ContainKey("alice").And.ContainValue(42);
/// config.Should().NotContainKey("legacy-flag");
/// </code>
/// </summary>
public class DictionaryAssertions<TKey, TValue> : AssertionsBase<IDictionary<TKey, TValue>?, DictionaryAssertions<TKey, TValue>>
{
    /// <summary>Creates the assertion for <paramref name="subject"/>.</summary>
    public DictionaryAssertions(IDictionary<TKey, TValue>? subject)
        : base(subject)
    {
    }
 
    /// <summary>Asserts the dictionary is non-null and contains <paramref name="key"/>.</summary>
    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainKey(TKey key, string because = "", params object[] becauseArgs)
    {
        Guard(Subject is not null && Subject.ContainsKey(key),
            $"Expected dictionary to contain key {Formatter.Format(key)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }
 
    /// <summary>Asserts the dictionary is null or does not contain <paramref name="key"/>.</summary>
    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKey(TKey key, string because = "", params object[] becauseArgs)
    {
        Guard(Subject is null || !Subject.ContainsKey(key),
            $"Expected dictionary not to contain key {Formatter.Format(key)}{Reason(because, becauseArgs)}, but it did.");
        return Continue;
    }
 
    /// <summary>Asserts the dictionary is non-null and contains <paramref name="value"/> among its values.</summary>
    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainValue(TValue value, string because = "", params object[] becauseArgs)
    {
        Guard(Subject is not null && Subject.Values.Contains(value),
            $"Expected dictionary to contain value {Formatter.Format(value)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }
 
    /// <summary>Asserts the dictionary is null or does not contain <paramref name="value"/> among its values.</summary>
    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValue(TValue value, string because = "", params object[] becauseArgs)
    {
        Guard(Subject is null || !Subject.Values.Contains(value),
            $"Expected dictionary not to contain value {Formatter.Format(value)}{Reason(because, becauseArgs)}, but it did.");
        return Continue;
    }
}