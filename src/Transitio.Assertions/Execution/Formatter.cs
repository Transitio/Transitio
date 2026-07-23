using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Transitio.Assertions;

/// <summary>
/// Renders values into short, readable fragments for failure messages, and provides the
/// in-house wildcard matcher used by string and exception-message assertions.
/// </summary>
internal static class Formatter
{
    private const int MaxCollectionItems = 10;

    /// <summary>
    /// Renders <paramref name="value"/> for a message: <c>&lt;null&gt;</c> for null, quoted
    /// strings/chars, <c>{a, b, c}</c> for sequences (capped), and the invariant string otherwise.
    /// </summary>
    public static string Format(object? value)
    {
        string? maybenull = null;
        string definitelynotnull = maybenull;
        switch (value)
        {
            case null:
                return "<null>";
            case string s:
                return $"\"{s}\"";
            case char c:
                return $"'{c}'";
            case bool b:
                return b ? "True" : "False";
            case Type t:
                return t.FullName ?? t.Name;
            case IEnumerable enumerable:
                return FormatEnumerable(enumerable);
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            default:
                return value.ToString() ?? "<null>";
        }
    }

    private static string FormatEnumerable(IEnumerable enumerable)
    {
        var builder = new StringBuilder("{");
        var count = 0;
        var truncated = false;

        foreach (var item in enumerable)
        {
            if (count == MaxCollectionItems)
            {
                truncated = true;
                break;
            }

            if (count > 0)
                builder.Append(", ");

            builder.Append(Format(item));
            count++;
        }

        if (truncated)
            builder.Append(", ...");

        builder.Append('}');
        return builder.ToString();
    }

    /// <summary>
    /// Matches <paramref name="value"/> against a wildcard <paramref name="pattern"/> where
    /// <c>*</c> matches any run of characters and <c>?</c> matches a single character.
    /// A null value only matches a null pattern.
    /// </summary>
    public static bool WildcardMatches(string? value, string pattern)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        if (value == null)
            return false;

        // Linear two-pointer match with backtracking indices: on a mismatch we return to the last
        // '*' and let it consume one more character. This is O(value.Length * pattern.Length) in the
        // worst case with no exponential recursion.
        var v = 0;
        var p = 0;
        var starPattern = -1;
        var starValue = 0;

        while (v < value.Length)
        {
            if (p < pattern.Length && (pattern[p] == '?' || pattern[p] == value[v]))
            {
                v++;
                p++;
            }
            else if (p < pattern.Length && pattern[p] == '*')
            {
                starPattern = p;
                starValue = v;
                p++;
            }
            else if (starPattern != -1)
            {
                p = starPattern + 1;
                starValue++;
                v = starValue;
            }
            else
            {
                return false;
            }
        }

        while (p < pattern.Length && pattern[p] == '*')
            p++;

        return p == pattern.Length;
    }
}
