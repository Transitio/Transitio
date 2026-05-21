using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class NestedMappingTests
{
    [Fact]
    public void Should_Map_Nested_Object()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderProfile>();
        });

        var mapper = config.BuildMapper();

        var order = new Order
        {
            Id = "ORD-1",
            Customer = new User { Name = "Hitesh", Age = 30 }
        };

        var result = mapper.Map<OrderDto>(order);

        Assert.Equal("ORD-1", result.Id);
        Assert.Equal("Hitesh", result.Customer.Name);
    }
}