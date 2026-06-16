# Getting Started

[← Back to README](../README.md)

## Install

```bash
dotnet add package Transitiio.Mapper
# optional: Microsoft.Extensions.DependencyInjection integration
dotnet add package Transitiio.Dependency
```

## Your first map (standalone)

Define a map and use the mapper. Members are matched by name; simple members are copied directly and complex/collection members are mapped recursively when a `CreateMap` exists.

```csharp
using Transitiio.Mapper;

public class User { public string Name { get; set; } = ""; public int Age { get; set; } }
public class UserDto { public string Name { get; set; } = ""; public int Age { get; set; } }

var config = new TransitiioMapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

IMapper mapper = config.BuildMapper();

var dto = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });
// dto.Name == "Hitesh", dto.Age == 30
```

## With dependency injection

If you use `Microsoft.Extensions.DependencyInjection`, register everything with `AddTransitiio(...)` and resolve `IMapper` — no manual `BuildMapper()` needed:

```csharp
services.AddTransitiio(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

// elsewhere
public class UserService(IMapper mapper)
{
    public UserDto ToDto(User user) => mapper.Map<UserDto>(user);
}
```

See [Dependency Injection](dependency-injection.md) for assembly scanning, lifetimes, keyed mappers, and startup validation.

## Mapping rules & limitations

- Only **public properties** are mapped. Public fields are not mapped automatically — expose data as properties, or populate fields with a custom [`ConvertUsing(...)`](converters.md) converter.
- Members are matched by **name**; "simple" members (primitives, `string`, `enum`, `DateTime`, `decimal`, `Guid`) are copied directly, and complex/collection members are mapped recursively when a corresponding `CreateMap` exists.
- For derived element types in collections, register a `CreateMap` for the concrete element type (maps are matched by exact type).

## Next steps

- [Core mapping features](mapping-features.md) – `ForMember`, `Ignore`, `Condition`, `ReverseMap`, nested & collection mapping, null handling, validation
- [Full object transformation](converters.md) – `ConvertUsing` (type / instance / delegate)
- [Inheritance & reusability](inheritance.md) – `Include` / `IncludeBase`
- [Dependency injection](dependency-injection.md)
