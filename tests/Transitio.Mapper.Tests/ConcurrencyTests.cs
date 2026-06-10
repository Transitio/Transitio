#nullable enable
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class ConcurrencyTests
{
    /// <summary>
    /// Regression test for concurrent use of a single (singleton) mapper.
    /// Nested mapping populates a per-expression cache at runtime; that cache must be
    /// thread-safe so parallel callers don't corrupt it or throw.
    /// </summary>
    [Fact]
    public void Should_Map_Nested_Objects_Concurrently_Without_Errors()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.BuildMapper();
        var results = new ConcurrentBag<OrderDto>();
        Parallel.For(0, 4000, i =>
        {
            var order = new Order
            {
                Id = "ORD-" + i,
                Customer = new User { Name = "User" + i, Age = i % 100 }
            };
            results.Add(mapper.Map<OrderDto>(order));
        });
        Assert.Equal(4000, results.Count);
        Assert.All(results, r =>
        {
            Assert.NotNull(r.Customer);
            Assert.StartsWith("ORD-", r.Id);
            Assert.StartsWith("User", r.Customer.Name);
        });
    }
}