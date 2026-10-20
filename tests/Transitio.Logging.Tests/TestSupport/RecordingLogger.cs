using Microsoft.Extensions.Logging;

namespace Transitio.Logging.Tests.TestSupport;

/// <summary>
/// A minimal, hand-rolled <see cref="ILogger"/> test double that records every <c>Log</c> call
/// (level, event id, formatted message, exception) along with a snapshot of the scope stack active
/// at the time — <c>BeginScope</c> pushes state onto the stack and pops it on <c>Dispose</c>,
/// mirroring how a real provider (e.g. the console logger) renders nested scopes.
/// </summary>
public sealed class RecordingLogger : ILogger
{
    private readonly List<object?> _activeScopes = new();

    public List<RecordedLogEntry> Entries { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        _activeScopes.Add(state);
        return new PopScope(this, state);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new RecordedLogEntry(
            logLevel, eventId, formatter(state, exception), exception, _activeScopes.ToArray()));
    }

    private sealed class PopScope : IDisposable
    {
        private readonly RecordingLogger _owner;
        private readonly object? _state;
        private bool _disposed;

        public PopScope(RecordingLogger owner, object? state)
        {
            _owner = owner;
            _state = state;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _owner._activeScopes.Remove(_state);
        }
    }
}

public sealed record RecordedLogEntry(
    LogLevel Level, EventId EventId, string Message, Exception? Exception, object?[] ScopeStack);

