using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Transitio.Mediator;

namespace Transitio.Mediator.Tests;

public sealed class Pinged : INotification
{
    public Pinged(string source) => Source = source;
    public string Source { get; }
}

// Two handlers for the same notification; both should fire on publish. They append to a shared list
// so tests can assert every handler ran.
public sealed class FirstPingedHandler : INotificationHandler<Pinged>
{
    private readonly NotificationLog _log;
    public FirstPingedHandler(NotificationLog log) => _log = log;

    public Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        _log.Entries.Add($"first:{notification.Source}");
        return Task.CompletedTask;
    }
}

public sealed class SecondPingedHandler : INotificationHandler<Pinged>
{
    private readonly NotificationLog _log;
    public SecondPingedHandler(NotificationLog log) => _log = log;

    public Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        _log.Entries.Add($"second:{notification.Source}");
        return Task.CompletedTask;
    }
}

// A separate notification used by the aggregate-exception test, kept off Pinged so the assembly
// scan test's Pinged publish stays clean.
public sealed class Buzzed : INotification
{
}

public sealed class RecordingBuzzedHandler : INotificationHandler<Buzzed>
{
    private readonly NotificationLog _log;
    public RecordingBuzzedHandler(NotificationLog log) => _log = log;

    public Task Handle(Buzzed notification, CancellationToken cancellationToken)
    {
        _log.Entries.Add("buzzed");
        return Task.CompletedTask;
    }
}

// Always throws, to verify a faulty handler does not stop the others.
public sealed class ThrowingBuzzedHandler : INotificationHandler<Buzzed>
{
    public Task Handle(Buzzed notification, CancellationToken cancellationToken)
        => throw new InvalidOperationException("boom");
}

// A notification with no registered handlers, to verify publish is a no-op.
public sealed class Unheard : INotification
{
}

// Shared sink injected into notification handlers.
public sealed class NotificationLog
{
    public List<string> Entries { get; } = new();
}
