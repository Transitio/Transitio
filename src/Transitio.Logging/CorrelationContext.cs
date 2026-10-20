using System;
using System.Threading;
 
namespace Transitio.Logging;
 
/// <summary>
/// Ambient correlation-id flow across an async call chain, independent of DI. Backed by an
/// <see cref="AsyncLocal{T}"/>, so the current value automatically flows to child <c>Task</c>s /
/// <c>async</c> continuations spawned from the scope that set it, and does not leak sideways to
/// unrelated concurrent flows (e.g. two requests handled concurrently on the same thread pool).
/// </summary>
/// <remarks>
/// This type is intentionally static rather than a DI-resolved service: correlation ids need to
/// flow through code that has no reference to a container (library internals, background
/// continuations, third-party middleware), and an <see cref="AsyncLocal{T}"/> is the only
/// mechanism in .NET that follows the logical call context across <c>await</c> boundaries without
/// explicit parameter passing. <see cref="LoggerExtensions.BeginCorrelationScope"/> is the usual
/// entry point; call <see cref="BeginScope"/> directly only when no logger scope is wanted
/// alongside the ambient id.
/// </remarks>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();
 
    /// <summary>
    /// Optional override for how a fresh correlation id is minted when <see cref="BeginScope"/> is
    /// called with no explicit id and none is already ambient. Set once at application startup —
    /// typically via <see cref="TransitioLoggingOptions.CorrelationIdGenerator"/> and
    /// <c>AddTransitioLogging</c> — since this is a static, process-wide configuration knob rather
    /// than a per-call parameter, matching the static/ambient nature of the rest of this type.
    /// Defaults to <see langword="null"/>, in which case <c>Guid.NewGuid().ToString("N")</c> is used.
    /// </summary>
    public static Func<string>? IdGenerator { get; set; }
 
    /// <summary>The correlation id ambient in the current logical call context, if any.</summary>
    public static string? Current => _current.Value;
 
    /// <summary>
    /// Begins a correlation scope, setting <see cref="Current"/> for the duration of the returned
    /// <see cref="IDisposable"/>'s lifetime and restoring the previous ambient value (including
    /// <see langword="null"/>, if there was none) on <c>Dispose</c> — scopes nest like a stack
    /// rather than clobbering an outer id.
    /// </summary>
    /// <param name="correlationId">
    /// An explicit id to use (e.g. read from an inbound request header). When <see langword="null"/>,
    /// the ambient <see cref="Current"/> id is reused if one is already set — so nested
    /// <see cref="BeginScope"/> calls within one logical operation share a single id — and a fresh
    /// id (via <see cref="IdGenerator"/>, or a new GUID by default) is minted only when nothing is
    /// ambient either.
    /// </param>
    public static IDisposable BeginScope(string? correlationId = null)
    {
        var previous = _current.Value;
        var id = correlationId ?? previous ?? (IdGenerator?.Invoke() ?? Guid.NewGuid().ToString("N"));
        _current.Value = id;
        return new RestoreScope(previous);
    }
 
    private sealed class RestoreScope : IDisposable
    {
        private readonly string? _previous;
        private bool _disposed;
 
        public RestoreScope(string? previous) => _previous = previous;
 
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _current.Value = _previous;
        }
    }
}