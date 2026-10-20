using Microsoft.Extensions.DependencyInjection;
using Transitio.Logging;
using Xunit;

namespace Transitio.Logging.Tests;

// See StaticCorrelationStateCollection in CorrelationContextTests.cs: this class also sets/resets
// the static CorrelationContext.IdGenerator, so it must run in the same non-parallelized collection.
[Collection(nameof(StaticCorrelationStateCollection))]
public class LoggingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTransitioLogging_RegistersOptionsAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddTransitioLogging();

        var provider = services.BuildServiceProvider();
        var options1 = provider.GetRequiredService<TransitioLoggingOptions>();
        var options2 = provider.GetRequiredService<TransitioLoggingOptions>();

        Assert.Same(options1, options2);
    }

    [Fact]
    public void AddTransitioLogging_NoConfigure_DoesNotThrow()
    {
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddTransitioLogging());

        Assert.Null(exception);
    }

    [Fact]
    public void AddTransitioLogging_SetsStaticIdGenerator_WhenConfigured()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddTransitioLogging(options => options.CorrelationIdGenerator = () => "configured-id");

            using var scope = CorrelationContext.BeginScope();

            Assert.Equal("configured-id", CorrelationContext.Current);
        }
        finally
        {
            CorrelationContext.IdGenerator = null; // static/process-wide: reset to avoid bleeding into other tests
        }
    }
}