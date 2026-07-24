using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Transitio.Assertions;

/// <summary>
/// Entry point for the fluent assertion API. Call <c>.Should()</c> on any value to begin an
/// assertion chain; the returned type is chosen by the compiler from the value's static type.
/// <code>
/// using Transitio.Assertions;
///
/// result.Should().Be(expected);
/// name.Should().StartWith("Tra");
/// items.Should().HaveCount(3).And.Contain("x");
/// age.Should().BeInRange(0, 120);
/// Action act = () =&gt; svc.Do();
/// act.Should().Throw&lt;InvalidOperationException&gt;().WithMessage("*failed*");
/// </code>
/// </summary>
public static class AssertionExtensions
{
    string? maybenull=null;
    string notnull = maybenull;
    /// <summary>Begins an assertion chain for any object or value (equality, nullability, type).</summary>
    /// <remarks>
    /// This fallback is intentionally non-generic. A generic <c>Should&lt;T&gt;(this T)</c> would win
    /// overload resolution over <see cref="Should{T}(System.Collections.Generic.IEnumerable{T})"/> for
    /// concrete collection types (an identity match beats the interface conversion), routing arrays and
    /// lists away from the collection assertions. Taking <see cref="object"/> keeps
    /// <c>IEnumerable&lt;T&gt;</c> the more specific match - at the cost of <c>Be(object)</c> rather than a
    /// type-checked <c>Be(T)</c>. This mirrors the design of established fluent-assertion libraries.
    /// </remarks>
    public static ObjectAssertions Should(this object? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a string.</summary>
    public static StringAssertions Should(this string? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a boolean.</summary>
    public static BooleanAssertions Should(this bool actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable boolean.</summary>
    public static BooleanAssertions Should(this bool? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a sequence.</summary>
    public static CollectionAssertions<T> Should<T>(this IEnumerable<T>? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for an action, to assert its exception behaviour.</summary>
    public static ActionAssertions Should(this Action action) => new(action);

    /// <summary>Begins an assertion chain for an async function, to assert its exception behaviour.</summary>
    public static AsyncActionAssertions Should(this Func<Task> action) => new(action);

    // Numeric value types - these non-generic overloads win over the generic object overload by
    // specificity, so numbers get ordering, range, sign and tolerance assertions.

    /// <summary>Begins an assertion chain for an <see cref="int"/>.</summary>
    public static NumericAssertions<int> Should(this int actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="int"/>.</summary>
    public static NumericAssertions<int> Should(this int? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="uint"/>.</summary>
    public static NumericAssertions<uint> Should(this uint actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="uint"/>.</summary>
    public static NumericAssertions<uint> Should(this uint? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="long"/>.</summary>
    public static NumericAssertions<long> Should(this long actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="long"/>.</summary>
    public static NumericAssertions<long> Should(this long? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="ulong"/>.</summary>
    public static NumericAssertions<ulong> Should(this ulong actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="ulong"/>.</summary>
    public static NumericAssertions<ulong> Should(this ulong? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="short"/>.</summary>
    public static NumericAssertions<short> Should(this short actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="short"/>.</summary>
    public static NumericAssertions<short> Should(this short? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="ushort"/>.</summary>
    public static NumericAssertions<ushort> Should(this ushort actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="ushort"/>.</summary>
    public static NumericAssertions<ushort> Should(this ushort? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="byte"/>.</summary>
    public static NumericAssertions<byte> Should(this byte actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="byte"/>.</summary>
    public static NumericAssertions<byte> Should(this byte? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for an <see cref="sbyte"/>.</summary>
    public static NumericAssertions<sbyte> Should(this sbyte actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="sbyte"/>.</summary>
    public static NumericAssertions<sbyte> Should(this sbyte? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="double"/>.</summary>
    public static NumericAssertions<double> Should(this double actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="double"/>.</summary>
    public static NumericAssertions<double> Should(this double? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="float"/>.</summary>
    public static NumericAssertions<float> Should(this float actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="float"/>.</summary>
    public static NumericAssertions<float> Should(this float? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="decimal"/>.</summary>
    public static NumericAssertions<decimal> Should(this decimal actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="decimal"/>.</summary>
    public static NumericAssertions<decimal> Should(this decimal? actualValue) => new(actualValue);

    // Non-numeric comparable value types - ordering and range assertions, no sign/tolerance.

    /// <summary>Begins an assertion chain for a <see cref="DateTime"/>.</summary>
    public static ComparableAssertions<DateTime> Should(this DateTime actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="DateTime"/>.</summary>
    public static ComparableAssertions<DateTime> Should(this DateTime? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="DateTimeOffset"/>.</summary>
    public static ComparableAssertions<DateTimeOffset> Should(this DateTimeOffset actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="DateTimeOffset"/>.</summary>
    public static ComparableAssertions<DateTimeOffset> Should(this DateTimeOffset? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="TimeSpan"/>.</summary>
    public static ComparableAssertions<TimeSpan> Should(this TimeSpan actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="TimeSpan"/>.</summary>
    public static ComparableAssertions<TimeSpan> Should(this TimeSpan? actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a <see cref="char"/>.</summary>
    public static ComparableAssertions<char> Should(this char actualValue) => new(actualValue);

    /// <summary>Begins an assertion chain for a nullable <see cref="char"/>.</summary>
    public static ComparableAssertions<char> Should(this char? actualValue) => new(actualValue);
}
