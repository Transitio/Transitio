using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
 
namespace Transitio.Logging;
 
/// <summary>
/// Extension methods that add structured, correlation-aware context on top of
/// <see cref="ILogger"/> — the main entry point for consumers of this package.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Begins a correlation scope: sets the ambient <see cref="CorrelationContext.Current"/> id
    /// (see <see cref="CorrelationContext.BeginScope"/> for how it is resolved) and opens an
    /// <see cref="ILogger"/> scope carrying it as a <c>CorrelationId</c> structured property, so
    /// every log entry written while the returned scope is active can be correlated back to the
    /// same logical operation.
    /// </summary>
    /// <param name="correlationId">
    /// An explicit id (e.g. from an inbound request header). When <see langword="null"/>, an
    /// already-ambient id is reused, or a fresh one is minted — see
    /// <see cref="CorrelationContext.BeginScope"/>.
    /// </param>
    public static IDisposable BeginCorrelationScope(this ILogger logger, string? correlationId = null)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
 
        var correlationScope = CorrelationContext.BeginScope(correlationId);
        var loggerScope = logger.BeginScope(new LogProperties(("CorrelationId", CorrelationContext.Current)));
        return new CompositeScope(loggerScope, correlationScope);
    }
 
    /// <summary>
    /// Opens an <see cref="ILogger"/> scope carrying the given key/value pairs as structured
    /// properties (see <see cref="LogProperties"/>), enriching every log entry written for the
    /// duration of the scope without repeating the properties on every call.
    /// </summary>
    /// <remarks>
    /// Pass two or more properties when calling this through the extension-method syntax. A call
    /// with exactly one tuple argument, e.g. <c>logger.BeginScope(("Key", value))</c>, binds to
    /// <c>ILogger</c>'s own instance method <c>BeginScope&lt;TState&gt;(TState state)</c> instead —
    /// C# always prefers an applicable instance method over an extension method, and the framework
    /// method is applicable for any single argument. The resulting scope still works (the state is
    /// just a boxed <c>ValueTuple</c> rather than a <see cref="LogProperties"/>), but loses the
    /// "Key=Value" <c>ToString()</c> rendering for plain-text providers. Passing two or more
    /// properties, or using <see cref="BeginCorrelationScope"/> for the single-property
    /// correlation-id case, avoids the ambiguity.
    /// </remarks>
    public static IDisposable? BeginScope(this ILogger logger, params (string Key, object? Value)[] properties)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
 
        return logger.BeginScope(new LogProperties(properties));
    }
 
    /// <summary>
    /// Times an operation: starts a <see cref="Stopwatch"/> and returns an <see cref="IDisposable"/>
    /// that, on <c>Dispose</c>, logs the elapsed time at the given <paramref name="level"/> using a
    /// structured message template — <c>"{OperationName} completed in {ElapsedMilliseconds}ms"</c> —
    /// so structured providers capture <c>OperationName</c> and <c>ElapsedMilliseconds</c> as
    /// separate fields rather than a single opaque interpolated string.
    /// </summary>
    /// <remarks>
    /// Intended for a <c>using</c> block wrapping the operation being timed:
    /// <c>using (logger.TimeOperation("PlaceOrder")) { ... }</c>.
    /// </remarks>
    public static IDisposable TimeOperation(
        this ILogger logger, string operationName, LogLevel level = LogLevel.Information)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
        if (string.IsNullOrEmpty(operationName))
            throw new ArgumentException("Operation name must not be null or empty.", nameof(operationName));
 
        return new OperationTimerScope(logger, operationName, level);
    }
 
    private sealed class CompositeScope : IDisposable
    {
        private readonly IDisposable? _loggerScope;
        private readonly IDisposable _correlationScope;
        private bool _disposed;
 
        public CompositeScope(IDisposable? loggerScope, IDisposable correlationScope)
        {
            _loggerScope = loggerScope;
            _correlationScope = correlationScope;
        }
 
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
 
            // Dispose the logger scope first, while the correlation id is still ambient (in case a
            // provider reads CorrelationContext.Current while unwinding the scope), then restore
            // the outer correlation id.
            _loggerScope?.Dispose();
            _correlationScope.Dispose();
        }
    }
 
    private sealed class OperationTimerScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly LogLevel _level;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;
 
        public OperationTimerScope(ILogger logger, string operationName, LogLevel level)
        {
            _logger = logger;
            _operationName = operationName;
            _level = level;
            _stopwatch = Stopwatch.StartNew();
        }
 
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
 
            _stopwatch.Stop();
            _logger.Log(
                _level,
                "{OperationName} completed in {ElapsedMilliseconds}ms",
                _operationName,
                _stopwatch.ElapsedMilliseconds);
        }
    }
}