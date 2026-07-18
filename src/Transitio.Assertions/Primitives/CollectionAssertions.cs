using System;
using System.Collections.Generic;
using System.Linq;

namespace Transitio.Assertions;

/// <summary>
/// Assertions for sequences (<see cref="IEnumerable{T}"/>). The source is enumerated once on
/// construction, so lazy queries and single-pass iterators are safe to assert on.
/// <code>
/// orders.Should().NotBeEmpty().And.HaveCount(3);
/// names.Should().Contain("Ada").And.OnlyContain(n =&gt; n.Length &gt; 0);
/// result.Should().BeEquivalentTo(new[] { 3, 2, 1 });
/// </code>
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class CollectionAssertions<T> : AssertionsBase<IEnumerable<T>?, CollectionAssertions<T>>
{
    private readonly IReadOnlyList<T>? _items;

    /// <summary>Creates the assertion, materialising <paramref name="subject"/> once.</summary>
    public CollectionAssertions(IEnumerable<T>? subject)
        : base(subject)
    {
        _items = subject?.ToList();
    }

    /// <summary>Asserts the collection is non-null and contains no elements.</summary>
    public AndConstraint<CollectionAssertions<T>> BeEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(_items is { Count: 0 },
            $"Expected collection to be empty{Reason(because, becauseArgs)}, but found {Formatter.Format(_items)}.");
        return Continue;
    }

    /// <summary>Asserts the collection is non-null and contains at least one element.</summary>
    public AndConstraint<CollectionAssertions<T>> NotBeEmpty(string because = "", params object[] becauseArgs)
    {
        Guard(_items is { Count: > 0 },
            $"Expected collection not to be empty{Reason(because, becauseArgs)}, but it was.");
        return Continue;
    }

    /// <summary>Asserts the collection has exactly <paramref name="expected"/> elements.</summary>
    public AndConstraint<CollectionAssertions<T>> HaveCount(int expected, string because = "", params object[] becauseArgs)
    {
        Guard(_items is not null && _items.Count == expected,
            $"Expected collection to have {expected} item(s){Reason(because, becauseArgs)}, but found {Formatter.Format(_items?.Count)}.");
        return Continue;
    }

    /// <summary>Asserts the collection has more than <paramref name="expected"/> elements.</summary>
    public AndConstraint<CollectionAssertions<T>> HaveCountGreaterThan(int expected, string because = "", params object[] becauseArgs)
    {
        Guard(_items is not null && _items.Count > expected,
            $"Expected collection to have more than {expected} item(s){Reason(because, becauseArgs)}, but found {Formatter.Format(_items?.Count)}.");
        return Continue;
    }

    /// <summary>Asserts the collection has fewer than <paramref name="expected"/> elements.</summary>
    public AndConstraint<CollectionAssertions<T>> HaveCountLessThan(int expected, string because = "", params object[] becauseArgs)
    {
        Guard(_items is not null && _items.Count < expected,
            $"Expected collection to have fewer than {expected} item(s){Reason(because, becauseArgs)}, but found {Formatter.Format(_items?.Count)}.");
        return Continue;
    }

    /// <summary>Asserts the collection contains <paramref name="expected"/> (default equality).</summary>
    public AndConstraint<CollectionAssertions<T>> Contain(T expected, string because = "", params object[] becauseArgs)
    {
        Guard(_items is not null && _items.Contains(expected),
            $"Expected collection {Formatter.Format(_items)} to contain {Formatter.Format(expected)}{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>Asserts the collection contains at least one element matching <paramref name="predicate"/>.</summary>
    public AndConstraint<CollectionAssertions<T>> Contain(Func<T, bool> predicate, string because = "", params object[] becauseArgs)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        Guard(_items is not null && _items.Any(predicate),
            $"Expected collection {Formatter.Format(_items)} to contain a matching item{Reason(because, becauseArgs)}, but none did.");
        return Continue;
    }

    /// <summary>Asserts the collection does not contain <paramref name="unexpected"/> (default equality).</summary>
    public AndConstraint<CollectionAssertions<T>> NotContain(T unexpected, string because = "", params object[] becauseArgs)
    {
        Guard(_items is null || !_items.Contains(unexpected),
            $"Expected collection {Formatter.Format(_items)} not to contain {Formatter.Format(unexpected)}{Reason(because, becauseArgs)}, but it did.");
        return Continue;
    }

    /// <summary>Asserts the collection contains exactly one element.</summary>
    public AndConstraint<CollectionAssertions<T>> ContainSingle(string because = "", params object[] becauseArgs)
    {
        Guard(_items is { Count: 1 },
            $"Expected collection to contain a single item{Reason(because, becauseArgs)}, but found {Formatter.Format(_items?.Count)}.");
        return Continue;
    }

    /// <summary>Asserts the collection is non-empty and every element matches <paramref name="predicate"/>.</summary>
    public AndConstraint<CollectionAssertions<T>> OnlyContain(Func<T, bool> predicate, string because = "", params object[] becauseArgs)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        Guard(_items is { Count: > 0 } && _items.All(predicate),
            $"Expected collection {Formatter.Format(_items)} to be non-empty and contain only matching items{Reason(because, becauseArgs)}, but it did not.");
        return Continue;
    }

    /// <summary>
    /// Asserts the collection has the same elements as <paramref name="expected"/> regardless of
    /// order (a multiset comparison using default equality).
    /// </summary>
    public AndConstraint<CollectionAssertions<T>> BeEquivalentTo(IEnumerable<T> expected, string because = "", params object[] becauseArgs)
    {
        if (expected == null)
            throw new ArgumentNullException(nameof(expected));

        var expectedList = expected.ToList();
        Guard(_items is not null && IsEquivalent(_items, expectedList),
            $"Expected collection {Formatter.Format(_items)} to be equivalent to {Formatter.Format(expectedList)}{Reason(because, becauseArgs)}, but it was not.");
        return Continue;
    }

    private static bool IsEquivalent(IReadOnlyList<T> actual, List<T> expected)
    {
        if (actual.Count != expected.Count)
            return false;

        var remaining = new List<T>(expected);
        var comparer = EqualityComparer<T>.Default;

        foreach (var item in actual)
        {
            var index = remaining.FindIndex(candidate => comparer.Equals(candidate, item));
            if (index < 0)
                return false;

            remaining.RemoveAt(index);
        }

        return remaining.Count == 0;
    }
}
