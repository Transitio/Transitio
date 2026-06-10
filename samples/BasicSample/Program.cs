using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;

var services = new ServiceCollection();

//Service consumed by a DI-resolved type converter (see DiConverterProfile)
services.AddSingleton<PricingService>();

// --------------------------------------------
// ✅ Configure Transitio
// --------------------------------------------
services.AddTransitio(cfg =>
{
    // Profiles
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<OrderProfile>();

    // ConvertUsing examples
    cfg.AddProfile<TypeConverterProfile>();
    cfg.AddProfile<InstanceConverterProfile>();
    cfg.AddProfile<DelegateConverterProfile>();
    cfg.AddProfile<DiConverterProfile>();

    // IncludeBase examples
    cfg.AddProfile<IncludeBasicProfile>();
    cfg.AddProfile<IncludeOverrideProfile>();
    cfg.AddProfile<IncludeMultiLevelProfile>();


    // Global settings
    cfg.SetIgnoreNullValues(true);

    // Custom mappings
    cfg.CreateMap<User, UserViewDto>()
       .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.ToUpper()))
       .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age >= 18));

    cfg.CreateMap<User, UserIgnoreDto>()
       .ForMember(dest => dest.Age, opt => opt.Ignore());

    cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();

    //Nested collection: Cart.Orders <List<Order>> auto maps to <List<OrderDto>>
    //and each Order's nested Customer is mapped too.
    cfg.CreateMap<Cart, CartDto>();
});

// --------------------------------------------
// ✅ Build Provider & Resolve Mapper
// --------------------------------------------
var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;

// --------------------------------------------
// ✅ Run Demos
// --------------------------------------------
RunOrderCollectionDemo(mapper);
RunUserMappingDemo(mapper);
RunReverseMappingDemo(mapper);
RunIgnoreNullDemo(mapper);
//Converters
RunTypeConverterDemo(mapper);
RunInstanceConverterDemo(mapper);
RunDelegateConverterDemo(mapper);
// IncludeBase
RunBasicInclude(mapper);
RunOverrideInclude(mapper);
RunMultiLevelInclude(mapper);
//Nested collection + DI converter
RunNestedCollectionDemo(mapper);
RunDiConverterDemo(mapper);



// ============================================
// ✅ Demo Methods
// ============================================

static void RunOrderCollectionDemo(IMapper mapper)
{
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

    Console.WriteLine("=== Order Collection Mapping ===");

    // List
    var orderDtos = mapper.Map<List<OrderDto>>(orders);
    Console.WriteLine("Order -> List<OrderDto>:");
    foreach (var dto in orderDtos)
    {
        Console.WriteLine($"- {dto.Id}: {dto.Customer.Name} ({dto.Customer.Age})");
    }

    Console.WriteLine();

    // Array
    var array = mapper.Map<OrderDto[]>(orders);
    Console.WriteLine("Order -> OrderDto[]:");
    Console.WriteLine($"- Length: {array.Length}");
    Console.WriteLine($"- First: {array[0].Id} -> {array[0].Customer.Name}");

    Console.WriteLine();

    // Interface
    var list = mapper.Map<IList<OrderDto>>(orders);
    Console.WriteLine("Order -> IList<OrderDto>:");
    Console.WriteLine($"- Count: {list.Count}");
    Console.WriteLine($"- Second: {list[1].Id} -> {list[1].Customer.Name}");

    Console.WriteLine();
}

static void RunUserMappingDemo(IMapper mapper)
{
    Console.WriteLine("=== User Mapping (ForMember + Condition) ===");

    var user = new User { Name = "Hitesh", Age = 16 };
    var result = mapper.Map<UserViewDto>(user);

    Console.WriteLine($"- DisplayName: {result.DisplayName}");
    Console.WriteLine($"- Age: {result.Age}");
    Console.WriteLine();

    Console.WriteLine("=== User Mapping (Ignore) ===");

    var ignoreResult = mapper.Map<UserIgnoreDto>(
        new User { Name = "Alice", Age = 30 });

    Console.WriteLine($"- Name: {ignoreResult.Name}");
    Console.WriteLine($"- Age: {ignoreResult.Age}");
    Console.WriteLine();
}

static void RunReverseMappingDemo(IMapper mapper)
{
    Console.WriteLine("=== Reverse Mapping ===");

    var dto = new UserDto { Name = "Jane", Age = 22 };
    var user = mapper.Map<User>(dto);

    Console.WriteLine($"- Name: {user.Name}");
    Console.WriteLine($"- Age: {user.Age}");
    Console.WriteLine();
}

static void RunIgnoreNullDemo(IMapper mapper)
{
    Console.WriteLine("=== IgnoreNullValues Demo ===");

    var source = new UserWithNullableName { Name = null };
    var result = mapper.Map<UserWithDefaultNameDto>(source);

    Console.WriteLine($"- Name: {result.Name}");
    Console.WriteLine();
}


static void RunTypeConverterDemo(IMapper mapper)
{
    Console.WriteLine("=== Type Converter ===");

    var input = new OrderInput
    {
        Amount = 1000,
        Currency = "INR",
        Country = "IN"
    };

    var result = mapper.Map<OrderDomain_Type>(input);

    Console.WriteLine($"Final Amount: {result.FinalAmount}");
    Console.WriteLine($"Region: {result.Region}");
    Console.WriteLine();
}

static void RunInstanceConverterDemo(IMapper mapper)
{
    Console.WriteLine("=== Instance Converter ===");

    var input = new OrderInput
    {
        Amount = 1000,
        Currency = "INR",
        Country = "IN"
    };

    var result = mapper.Map<OrderDomain_Instance>(input);

    Console.WriteLine($"Final Amount: {result.FinalAmount}");
    Console.WriteLine($"Region: {result.Region}");
    Console.WriteLine();
}

static void RunDelegateConverterDemo(IMapper mapper)
{
    Console.WriteLine("=== Delegate Converter ===");

    var input = new OrderInput
    {
        Amount = 1000,
        Currency = "INR",
        Country = "IN"
    };

    var result = mapper.Map<OrderDomain_Delegate>(input);

    Console.WriteLine($"Final Amount: {result.FinalAmount}");
    Console.WriteLine($"Region: {result.Region}");
    Console.WriteLine();
}


static void RunBasicInclude(IMapper mapper)
{
    Console.WriteLine("=== Include: Basic ===");

    var employee = new Employee
    {
        Name = "Hitesh",
        Department = "IT"
    };

    var dto = mapper.Map<EmployeeDto>(employee);

    Console.WriteLine($"Name: {dto.Name}");
    Console.WriteLine($"Department: {dto.Department}");
    Console.WriteLine();
}

static void RunOverrideInclude(IMapper mapper)
{
    Console.WriteLine("=== Include: Override ===");

    var employee = new Employee
    {
        Name = "Hitesh",
        Department = "Engineering"
    };

    var dto = mapper.Map<EmployeeDto>(employee);

    Console.WriteLine($"Name: {dto.Name}"); // Should be overridden
    Console.WriteLine($"Department: {dto.Department}");
    Console.WriteLine();
}

static void RunMultiLevelInclude(IMapper mapper)
{
    Console.WriteLine("=== Include: Multi-Level ===");

    var manager = new Manager
    {
        Name = "Hitesh",
        Department = "Architecture",
        TeamSize = 10
    };

    var dto = mapper.Map<ManagerDto>(manager);

    Console.WriteLine($"Name: {dto.Name}");
    Console.WriteLine($"Department: {dto.Department}");
    Console.WriteLine($"TeamSize: {dto.TeamSize}");
    Console.WriteLine();
}

static void RunNestedCollectionDemo(IMapper mapper)
{
    Console.WriteLine("=== Nested Collection (Cart -> CartDto) ===");
    var cart = new Cart
    {
        Id = "CART-1",
        Orders = new List<Order>
        {
            new Order
            {
                Id = "ORD-1",
                Customer = new User { Name = "Hitesh", Age = 30 }
            },
            new Order
            {
                Id = "ORD-2",
                Customer = new User { Name = "John", Age = 22 }
            }
        }
    };

    var dto = mapper.Map<CartDto>(cart);

    Console.WriteLine($"Cart: {dto.Id} ({dto.Orders.Count} orders)");
    foreach (var o in dto.Orders)
    {
        Console.WriteLine($"- {o.Id}: {o.Customer.Name} ({o.Customer.Age})");
    }
    Console.WriteLine();
}
static void RunDiConverterDemo(IMapper mapper)
{
    Console.WriteLine("=== DI-Resolved Type Converter ===");
    var input = new OrderInput
    {
        Amount = 1000,
        Currency = "USD",
        Country = "US"
    };
    // DiPricingConverter has a constructor dependency (Pricing Service) resolved by the container.
    var result = mapper.Map<OrderDomain_DI>(input);

    Console.WriteLine($"Final Amount: {result.FinalAmount}");
    Console.WriteLine($"Region: {result.Region}");
    Console.WriteLine();
}