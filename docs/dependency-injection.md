# Dependency Injection

[← Back to README](../README.md) · [Getting Started](getting-started.md)

`Transitio.Dependency` registers Transitio with `Microsoft.Extensions.DependencyInjection`.

```bash
dotnet add package Transitio.Dependency
```

## Inline configuration

```csharp
services.AddTransitio(cfg =>
{
    cfg.CreateMap<User, UserDto>();
    cfg.CreateMap<Order, OrderDto>();
});
```

## Resolving the mapper

`AddTransitio` registers `IMapper` directly, so the recommended approach is to constructor-inject `IMapper`:

```csharp
public class UserService
{
    private readonly IMapper _mapper;

    public UserService(IMapper mapper) => _mapper = mapper;

    public UserDto ToDto(User user) => _mapper.Map<UserDto>(user);
}
```

You can also resolve it explicitly, or go through the `TransitioDependency` accessor:

```csharp
var mapper = provider.GetRequiredService<IMapper>();
// or
var mapper = provider.GetRequiredService<TransitioDependency>().Mapping.Mapper;

var dto = mapper.Map<UserDto>(user);
```

## Assembly scanning for profiles

Instead of registering every map inline, scan one or more assemblies for `MappingProfile` types (concrete classes with a parameterless constructor are discovered automatically):

```csharp
// Scan the given assemblies
services.AddTransitio(typeof(UserProfile).Assembly);
```

// Inline configuration PLUS assembly scanning
```csharp
services.AddTransitio(
    cfg => cfg.SetIgnoreNullValues(true),
    typeof(UserProfile).Assembly);
```

## Configurable lifetime (scoped converters)

By default the mapper is registered as a **singleton**, which keeps the compiled-mapping cache shared across the whole application. When a `ConvertUsing<TConverter>()` converter depends on a **scoped** service (for example a `DbContext`), register Transitio with a `Scoped` (or `Transient`) lifetime so the converter is instantiated from the same scope as the resolved mapper:

```csharp
services.AddScoped<MyDbContext>();

services.AddTransitio(
    ServiceLifetime.Scoped,
    cfg => cfg.CreateMap<Order, OrderDto>().ConvertUsing<DbBackedConverter>());

// resolve TransitioDependency from a scope; the converter gets that scope's DbContext
using var scope = provider.CreateScope();
var mapper = scope.ServiceProvider.GetRequiredService<TransitioDependency>().Mapping.Mapper;
```

### Notes on non-singleton lifetimes

> Non-singleton lifetimes rebuild the lightweight mapper wrapper (and its compiled-mapping cache) per scope. The mapping configuration itself is built once and shared.

## Fail-fast configuration validation

Call `ValidateConfiguration()` inside the config delegate to validate every map eagerly. The configuration is checked as soon as it is built — i.e. during `AddTransitio` at startup — so a missing or type-mismatched property throws immediately instead of at the first `Map` call. Validation runs *after* all inline maps and any assembly-scanned profiles have been registered:

```csharp
services.AddTransitio(cfg =>
{
    cfg.ValidateConfiguration(); // opt in to fail-fast validation
    cfg.CreateMap<User, UserDto>();
    cfg.CreateMap<Order, OrderDto>();
});
// throws InvalidOperationException here if any map is invalid
```

It also works with the standalone mapper:

```csharp
var config = new TransitioMapperConfiguration(cfg =>
{
    cfg.ValidateConfiguration();
});
```

> Maps backed by a `ConvertUsing(...)` converter are exempt from property-shape checks, since the converter replaces property-by-property mapping. You can also still call `config.AssertConfigurationIsValid()` manually when using the standalone mapper.

## Keyed mappers

Register multiple independent mapper configurations side by side under different keys with `AddKeyedTransitio(key, ...)`, then resolve them via .NET keyed services. Each key gets its own isolated configuration:

```csharp
services.AddKeyedTransitio("public", cfg =>
    cfg.CreateMap<User, UserDto>()
       .ForMember(d => d.Name, o => o.MapFrom(s => Mask(s.Name))));

services.AddKeyedTransitio("internal", cfg =>
    cfg.CreateMap<User, UserDto>());
```

Resolve explicitly:

```csharp
var publicMapper = provider.GetRequiredKeyedService<IMapper>("public");
var internalMapper = provider.GetRequiredKeyedService<IMapper>("internal");
```

...or constructor-inject a specific key with `[FromKeyedServices]`:

```csharp
public class ReportService
{
    private readonly IMapper _mapper;

    public ReportService([FromKeyedServices("public")] IMapper mapper) => _mapper = mapper;
}
```

All `AddKeyedTransitio` overloads mirror `AddTransitio` — they accept inline configuration, assembly scanning, and an explicit `ServiceLifetime`:

```csharp
services.AddKeyedTransitio("reporting", ServiceLifetime.Scoped,
    cfg => cfg.CreateMap<Order, OrderDto>(),
    typeof(ReportingProfile).Assembly);
```
