# Core Mapping Features

[← Back to README](../README.md) · [Getting Started](getting-started.md)

## Profiles

Group related maps into a `MappingProfile` for modular configuration:

```csharp
public class UserProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<User, UserDto>().ReverseMap();
    }
}

var config = new TransitioMapperConfiguration(cfg =>
{
    cfg.AddProfile<UserProfile>();
    // or: cfg.AddProfiles(new UserProfile(), new OrderProfile());
});
```

> With DI, profiles can be discovered automatically by scanning assemblies — see
> [Assembly scanning](dependency-injection.md#assembly-scanning-for-profiles).

## Custom member mapping – `ForMember(...).MapFrom(...)`

```csharp
cfg.CreateMap<User, UserViewDto>()
    .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.ToUpper()));
```

## Ignore a property – `ForMember(...).Ignore()`

```csharp
cfg.CreateMap<User, UserDto>()
    .ForMember(dest => dest.Age, opt => opt.Ignore()); // Age keeps its default
```

## Conditional mapping – `ForMember(...).Condition(...)`

```csharp
cfg.CreateMap<User, UserViewDto>()
    // Age is only mapped when the source user is an adult; otherwise it stays default.
    .ForMember(dest => dest.Age, opt => opt.Condition(src => src.Age >= 18));
```

## Reverse mapping – `ReverseMap()`

```csharp
cfg.CreateMap<User, UserDto>().ReverseMap();

var dto = mapper.Map<UserDto>(user);      // User -> UserDto
var back = mapper.Map<User>(dto);         // UserDto -> User
```

## Preserve destination defaults – `SetIgnoreNullValues(true)`

```csharp
cfg.SetIgnoreNullValues(true);
cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();
```

## Configuration validation – `AssertConfigurationIsValid()`

Validate that every destination property has a compatible source (or is handled by
`ForMember` / `Ignore` / a converter). Useful in a unit test or at startup:

```csharp
var config = new TransitioMapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

config.AssertConfigurationIsValid(); // throws InvalidOperationException if anything is off
```

> With DI you can opt in to the same checks at startup via `cfg.ValidateConfiguration()` –
> see [fail-fast configuration validation](dependency-injection.md#fail-fast-configuration-validation)

## Nested & collection mapping

Nested objects and collections map automatically when a `CreateMap` exists for the element types — including `List<T>`, arrays, and interface destinations like `IList<T>`:

```csharp
cfg.CreateMap<User, UserDto>();
cfg.CreateMap<Order, OrderDto>(); // Order has a User Customer; Cart has List<Order> Orders
cfg.CreateMap<Cart, CartDto>();

var cartDto = mapper.Map<CartDto>(cart);        // nested Orders + each Customer mapped
var dtos    = mapper.Map<List<OrderDto>>(orders); // List -> List
var array   = mapper.Map<OrderDto[]>(orders);     // IEnumerable -> array
var asList  = mapper.Map<IList<OrderDto>>(orders); // IEnumerable -> interface
```

## Preserve destination defaults – `SetIgnoreNullValues(true)`

When enabled, a `null` source value does not overwrite the destination property, so the destination keeps its default:

```csharp
var config = new TransitioMapperConfiguration(cfg =>
{
    cfg.SetIgnoreNullValues(true);
    cfg.CreateMap<UserWithNullableName, UserWithDefaultNameDto>();
});
```
