using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Transitio.Logging.Tests.TestSupport;
using Xunit;
 
namespace Transitio.Logging.Tests;
 
public class LoggerExtensionsTests
{
    [Fact]
    public void BeginCorrelationScope_OpensLoggerScopeWithCorrelationId()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginCorrelationScope("corr-1"))
        {
            logger.LogInformation("inside scope");
        }
 
        var entry = Assert.Single(logger.Entries);
        var scopeState = Assert.Single(entry.ScopeStack);
        Assert.Contains("CorrelationId=corr-1", scopeState!.ToString());
    }
 
    [Fact]
    public void BeginCorrelationScope_Dispose_ClearsAmbientCorrelationId()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginCorrelationScope("corr-2"))
        {
            Assert.Equal("corr-2", CorrelationContext.Current);
        }
 
        Assert.Null(CorrelationContext.Current);
    }
 
    [Fact]
    public void BeginScope_Properties_TwoOrMore_UsesLogPropertiesToString()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginScope(("OrderId", 42), ("CustomerId", "ada")))
        {
            logger.LogInformation("inside scope");
        }
 
        var entry = Assert.Single(logger.Entries);
        var scopeState = Assert.Single(entry.ScopeStack);
        Assert.Equal("OrderId=42, CustomerId=ada", scopeState!.ToString());
    }
 
    [Fact]
    public void TimeOperation_Dispose_LogsWithTemplateAndArgs()
    {
        var logger = new RecordingLogger();
 
        using (logger.TimeOperation("PlaceOrder"))
        {
            Thread.Sleep(1);
        }
 
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("PlaceOrder completed in", entry.Message);
        Assert.Contains("ms", entry.Message);
    }
 
    [Fact]
    public void TimeOperation_UsesConfiguredLogLevel()
    {
        var logger = new RecordingLogger();
 
        using (logger.TimeOperation("PlaceOrder", LogLevel.Debug))
        {
        }
 
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Debug, entry.Level);
    }
}