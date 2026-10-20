using Microsoft.Extensions.DependencyInjection;

namespace Transitio.Logging;

public static class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="TransitioLoggingOptions"/> as a singleton, built from the optional
    /// <paramref name="configure"/> delegate, and applies any options that require a one-time,
    /// process-wide assignment — currently just
    /// <see cref="TransitioLoggingOptions.CorrelationIdGenerator"/>, copied onto the static
    /// <see cref="CorrelationContext.IdGenerator"/> so <see cref="CorrelationContext.BeginScope"/>
    /// picks it up even outside of DI-resolved code.
    /// </summary>
    /// <remarks>
    /// This method does not register an <c>ILoggerFactory</c> or any logging provider — Transitio.Logging
    /// is a thin wrapper over <c>Microsoft.Extensions.Logging</c>, not a backend. Call
    /// <c>services.AddLogging(...)</c> (or your host's default logging setup) separately.
    /// </remarks>
    public static IServiceCollection AddTransitioLogging(
        this IServiceCollection services,
        Action<TransitioLoggingOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var options = new TransitioLoggingOptions();
        configure?.Invoke(options);

        if (options.CorrelationIdGenerator != null)
            CorrelationContext.IdGenerator = options.CorrelationIdGenerator;

        services.AddSingleton(options);

        return services;
    }
}
