using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Transitio.Logging.Tests.TestSupport;
using Xunit;
 
namespace Transitio.Logging.Tests;
 
// LogProperties itself is internal, so these tests exercise it through the public surface that
// produces it (LoggerExtensions.BeginScope) — exactly how a real logger provider would interact
// with the opaque scope state object (ToString(), or casting to the public BCL interface it
// implements), rather than naming the internal type directly.
public class LogPropertiesTests
{
    [Fact]
    public void Enumeration_YieldsAllPairs_InOrder()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginScope(("A", 1), ("B", "two")))
        {
            logger.LogInformation("x");
        }
 
        var entry = Assert.Single(logger.Entries);
        var state = Assert.Single(entry.ScopeStack);
        var pairs = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(state);
 
        Assert.Equal(2, pairs.Count);
        Assert.Equal(new KeyValuePair<string, object?>("A", 1), pairs[0]);
        Assert.Equal(new KeyValuePair<string, object?>("B", "two"), pairs[1]);
    }
 
    [Fact]
    public void Count_MatchesInputLength()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginScope(("A", 1), ("B", 2), ("C", 3)))
        {
            logger.LogInformation("x");
        }
 
        var entry = Assert.Single(logger.Entries);
        var state = Assert.Single(entry.ScopeStack);
        var pairs = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(state);
 
        Assert.Equal(3, pairs.Count);
    }
 
    [Fact]
    public void ToString_RendersKeyEqualsValue_CommaSeparated()
    {
        var logger = new RecordingLogger();
 
        using (logger.BeginScope(("A", 1), ("B", "two")))
        {
            logger.LogInformation("x");
        }
 
        var entry = Assert.Single(logger.Entries);
        var state = Assert.Single(entry.ScopeStack);
 
        Assert.Equal("A=1, B=two", state!.ToString());
    }
}