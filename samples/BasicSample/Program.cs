using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;

var services = new ServiceCollection();

// ✅ Register Transitio with profiles
services.AddTransitio(cfg =>
{
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<OrderProfile>();
});

// ✅ Build provider
var provider = services.BuildServiceProvider();

// ✅ Get dependency wrapper
var dep = provider.GetRequiredService<TransitioDependency>();

// ✅ Sample data
var orders = new List<Order>
{
    new Order
    {
        Id = "ORD-1",
        Customer = new User { Name = "Hitesh", Age = 30 }
    },
    new Order
    {
        Id = "ORD-2",
        Customer = new User { Name = "John", Age = 25 }
    }
};

// ✅ Map (collection + nested)
var result = dep.Mapping.Mapper.Map<List<OrderDto>>(orders);

// ✅ Output
foreach (var dto in result)
{
    Console.WriteLine($"{dto.Id} - {dto.Customer.Name}");
}