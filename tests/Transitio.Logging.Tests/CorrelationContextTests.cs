using System.Threading.Tasks;
using Transitio.Logging;
using Xunit;
 
namespace Transitio.Logging.Tests;
 
// CorrelationContext.IdGenerator is a static, process-wide field. This collection forces every
// test class that sets/resets it (here and LoggingServiceCollectionExtensionsTests) to run
// sequentially rather than in parallel - xUnit's default is to parallelize across test classes,
// which would otherwise let one test's reset race another's assertion.
[CollectionDefinition(nameof(StaticCorrelationStateCollection), DisableParallelization = true)]
public class StaticCorrelationStateCollection
{
}
 
[Collection(nameof(StaticCorrelationStateCollection))]
public class CorrelationContextTests
{
    [Fact]
    public void BeginScope_NoArgs_MintsNewGuid()
    {
        using var scope = CorrelationContext.BeginScope();
 
        Assert.NotNull(CorrelationContext.Current);
        Assert.Equal(32, CorrelationContext.Current!.Length); // Guid.ToString("N") is 32 hex chars
    }
 
    [Fact]
    public void BeginScope_ExplicitId_SetsCurrent()
    {
        using var scope = CorrelationContext.BeginScope("explicit-id");
 
        Assert.Equal("explicit-id", CorrelationContext.Current);
    }
 
    [Fact]
    public void BeginScope_Nested_ReusesAmbientId()
    {
        using var outer = CorrelationContext.BeginScope("outer-id");
        using var inner = CorrelationContext.BeginScope(); // no explicit id -> reuses ambient
 
        Assert.Equal("outer-id", CorrelationContext.Current);
    }
 
    [Fact]
    public void BeginScope_Dispose_RestoresPreviousValue()
    {
        using (CorrelationContext.BeginScope("outer-id"))
        {
            using (CorrelationContext.BeginScope("inner-id"))
            {
                Assert.Equal("inner-id", CorrelationContext.Current);
            }
 
            Assert.Equal("outer-id", CorrelationContext.Current);
        }
 
        Assert.Null(CorrelationContext.Current);
    }
 
    [Fact]
    public void BeginScope_Dispose_RestoresNull_WhenNoOuterScope()
    {
        Assert.Null(CorrelationContext.Current);
 
        using (CorrelationContext.BeginScope("only-id"))
        {
            Assert.Equal("only-id", CorrelationContext.Current);
        }
 
        Assert.Null(CorrelationContext.Current);
    }
 
    [Fact]
    public void IdGenerator_UsedWhenSet_AndNothingAmbient()
    {
        CorrelationContext.IdGenerator = () => "generated-id";
        try
        {
            using var scope = CorrelationContext.BeginScope();
 
            Assert.Equal("generated-id", CorrelationContext.Current);
        }
        finally
        {
            CorrelationContext.IdGenerator = null; // static/process-wide: reset to avoid bleeding into other tests
        }
    }
 
    [Fact]
    public async Task BeginScope_SiblingAsyncFlows_DoNotShareCorrelationId()
    {
        string? first = null;
        string? second = null;
 
        var task1 = Task.Run(() =>
        {
            using var scope = CorrelationContext.BeginScope("flow-1");
            first = CorrelationContext.Current;
        });
 
        var task2 = Task.Run(() =>
        {
            using var scope = CorrelationContext.BeginScope("flow-2");
            second = CorrelationContext.Current;
        });
 
        await Task.WhenAll(task1, task2);
 
        Assert.Equal("flow-1", first);
        Assert.Equal("flow-2", second);
    }
}