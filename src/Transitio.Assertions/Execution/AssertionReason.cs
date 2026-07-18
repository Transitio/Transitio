using System;
using System.Globalization;

namespace Transitio.Assertions;

/// <summary>
/// Builds the optional "because" clause shared by every assertion's failure message.
/// </summary>
internal static class AssertionReason
{
    /// <summary>
    /// Turns the optional <paramref name="because"/> reason into a message fragment such as
    /// <c>" because the id was reused"</c> (leading space included), or an empty string when no
    /// reason was supplied. A "because" prefix is added when the caller omits it.
    /// </summary>
    public static string Build(string because, object[] becauseArgs)
    {
        if (string.IsNullOrEmpty(because))
            return string.Empty;

        var text = becauseArgs is { Length: > 0 }
            ? string.Format(CultureInfo.InvariantCulture, because, becauseArgs)
            : because;

        text = text.TrimStart();
        if (!text.StartsWith("because", StringComparison.OrdinalIgnoreCase))
            text = "because " + text;

        return " " + text;
    }
}
