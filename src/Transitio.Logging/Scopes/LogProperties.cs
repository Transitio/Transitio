using System.Collections;
using System.Text;

namespace Transitio.Logging;

/// <summary>
/// The <c>TState</c> passed to <c>ILogger.BeginScope{TState}</c> by <see cref="LoggerExtensions"/>.
/// Implements <see cref="IReadOnlyList{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> so
/// structured providers (e.g. ones that serialize scope state to JSON) can enumerate the
/// individual properties, while <see cref="ToString"/> renders a flat "Key1=Value1, Key2=Value2"
/// string for plain-text providers that just call <c>ToString()</c> on the scope state.
/// </summary>
internal sealed class LogProperties : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly KeyValuePair<string, object?>[] _properties;
    private string? _text;

    public LogProperties(params (string Key, object? Value)[] properties)
    {
        _properties = new KeyValuePair<string, object?>[properties.Length];
        for (var i = 0; i < properties.Length; i++)
            _properties[i] = new KeyValuePair<string, object?>(properties[i].Key, properties[i].Value);
    }

    public int Count => _properties.Length;

    public KeyValuePair<string, object?> this[int index] => _properties[index];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<string, object?>>)_properties).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Renders as "Key1=Value1, Key2=Value2" for plain-text logger providers that format scope
    /// state by calling <see cref="object.ToString"/> on it. Computed once and cached — a given
    /// instance is immutable after construction and may be formatted repeatedly (once per
    /// provider) for a single log entry.
    /// </summary>
    public override string ToString()
    {
        if (_text != null)
            return _text;

        var builder = new StringBuilder();
        for (var i = 0; i < _properties.Length; i++)
        {
            if (i > 0)
                builder.Append(", ");
            builder.Append(_properties[i].Key).Append('=').Append(_properties[i].Value);
        }

        return _text = builder.ToString();
    }
}
