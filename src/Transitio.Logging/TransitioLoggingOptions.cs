
namespace Transitio.Logging;

/// <summary>
/// Application-startup-time configuration for Transitio.Logging, applied via
/// <c>AddTransitioLogging</c>.
/// </summary>
/// <remarks>
/// This is deliberately not resolved from DI per-request: <see cref="CorrelationContext"/> itself
/// must remain a static, ambient (<see cref="System.Threading.AsyncLocal{T}"/>-backed) type to
/// flow correctly across async boundaries without a container in scope, so the one option it
/// exposes is applied once, at startup, as a static assignment rather than injected per call.
/// </remarks>
public sealed class TransitioLoggingOptions
{
    /// <summary>
    /// Optional override for how <see cref="CorrelationContext.BeginScope"/> mints a fresh
    /// correlation id when none is passed explicitly and none is already ambient. Defaults to
    /// <see langword="null"/>, in which case <c>Guid.NewGuid().ToString("N")</c> is used.
    /// </summary>
    public Func<string>? CorrelationIdGenerator { get; set; }
}
