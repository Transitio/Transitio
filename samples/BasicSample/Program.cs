using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;

var services = new ServiceCollection();

// Register Transitio and add sample mappings
services.AddTransitio(cfg =>
{
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<OrderProfile>();
    cfg.SetIgnoreNullValues(true);

    // Custom member mapping with ForMember / MapFrom
    cfg.CreateMap<User, UserViewDto>()
       .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.ToUpper()))
       .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age >= 18));

    // Ignore a destination property
    cfg.CreateMap<User, UserIgnoreDto>()
       .ForMember(dest => dest.Age, opt => opt.Ignore());

    // Demonstrate IgnoreNullValues with default destination initializers
    cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();
});

var provider = services.BuildServiceProvider();
var dep = provider.GetRequiredService<TransitioDependency>();

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
        Customer = new User { Name = "John", Age = 17 }
    }
};

Console.WriteLine("Order -> OrderDto:");
var orderDtos = dep.Mapping.Mapper.Map<List<OrderDto>>(orders);
foreach (var dto in orderDtos)
{
    Console.WriteLine($"- {dto.Id}: {dto.Customer.Name} ({dto.Customer.Age})");
}

Console.WriteLine();

Console.WriteLine("Order -> OrderDto[] (Array destination):");
var orderDtoArray = dep.Mapping.Mapper.Map<OrderDto[]>(orders);
Console.WriteLine($"- Array length: {orderDtoArray.Length}");
Console.WriteLine($"- First order: {orderDtoArray[0].Id} -> {orderDtoArray[0].Customer.Name}");

Console.WriteLine();

Console.WriteLine("Order -> IList<OrderDto> (Interface destination):");
var orderDtoList = dep.Mapping.Mapper.Map<IList<OrderDto>>(orders);
Console.WriteLine($"- Count: {orderDtoList.Count}");
Console.WriteLine($"- Second order: {orderDtoList[1].Id} -> {orderDtoList[1].Customer.Name}");

Console.WriteLine();

var user = new User { Name = "Hitesh", Age = 16 };
var userView = dep.Mapping.Mapper.Map<UserViewDto>(user);
Console.WriteLine("User -> UserViewDto (ForMember + Condition):");
Console.WriteLine($"- DisplayName: {userView.DisplayName}");
Console.WriteLine($"- Age: {userView.Age}");

Console.WriteLine();

var userIgnore = dep.Mapping.Mapper.Map<UserIgnoreDto>(new User { Name = "Alice", Age = 30 });
Console.WriteLine("User -> UserIgnoreDto (Ignore Age):");
Console.WriteLine($"- Name: {userIgnore.Name}");
Console.WriteLine($"- Age: {userIgnore.Age}");

Console.WriteLine();

var reverseUserDto = new UserDto { Name = "Jane", Age = 22 };
var reverseUser = dep.Mapping.Mapper.Map<User>(reverseUserDto);
Console.WriteLine("UserDto -> User (ReverseMap):");
Console.WriteLine($"- Name: {reverseUser.Name}");
Console.WriteLine($"- Age: {reverseUser.Age}");

Console.WriteLine();

var nullableUser = new UserWithNullableName { Name = null };
var defaultNameDto = dep.Mapping.Mapper.Map<UserWithDefaultNameDto>(nullableUser);
Console.WriteLine("UserWithNullableName -> UserWithDefaultNameDto (IgnoreNullValues):");
Console.WriteLine($"- Name: {defaultNameDto.Name}");