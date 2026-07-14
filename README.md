# Transitio

[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](https://www.nuget.org/packages/Transitio.Mapper)  
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](https://www.nuget.org/packages/Transitio.Dependency)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Validation)](https://www.nuget.org/packages/Transitio.Validation)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Mediator)](https://www.nuget.org/packages/Transitio.Mediator)

**Transitio** is a lightweight, high-performance object mapping framework for .NET with support for profiles, nested mapping, dependency injection, object validation and an in-process mediator.

---

## 📦 Installation

```bash
dotnet add package Transitio.Mapper
# optional: Microsoft.Extensions.DependencyInjection integration
dotnet add package Transitio.Dependency
# optional: standalone object validation
dotnet add package Transitio.Validation
# optional: standalone in-process mediator
dotnet add package Transitio.Mediator
```

---

## 🚀 Quick start

```csharp
using Transitio.Mapper;

public class User { public string Name { get; set; } = ""; public int Age { get; set; } }
public class UserDto { public string Name { get; set; } = ""; public int Age { get; set; } }

var config = new TransitioMapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

IMapper mapper = config.BuildMapper();

var dto = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });
// dto.Name == "Hitesh", dto.Age == 30
```

Using dependency injection? Register with `services.AddTransitio(...)` and inject `IMapper`.  
See the [Getting Started](https://github.com/Transitio/Transitio/tree/main/docs/getting-started.md) guide.

---

## ✨ Features

- ⚡ Expression-based high-performance mapping  
- 🧩 [Profiles](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#profiles) for modular configuration  
- 🔗 [Nested object mapping](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#nested--collection-mapping) (including nested collection properties)  
- 📦 [Collection mapping](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#nested--collection-mapping) (List → List, IEnumerable → array, IEnumerable → interface)  
- 🧵 Thread-safe: a single mapper instance can be shared as a singleton  
- 🔧 [Custom member mapping](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#custom-member-mapping--formembermapfrom),  
  [Ignore](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#ignore-a-property--formemberignore),  
  [Conditional mapping](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#conditional-mapping--formembercondition),  
  and [Reverse mapping](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#reverse-mapping--reversemap)  
- 🔁 [Full object transformation](https://github.com/Transitio/Transitio/tree/main/docs/converters.md) with `ConvertUsing(...)` (type / instance / delegate)  
  and context-aware mapping via `IMappingContext`  
- 🧬 [Inheritance & reuse](https://github.com/Transitio/Transitio/tree/main/docs/inheritance.md) with `Include` / `IncludeBase`  
- 🧪 [Null-value handling](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#preserve-destination-defaults--setignorenullvaluestrue)  
  and [configuration validation](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md#configuration-validation--assertconfigurationisvalid)  
- 📦 [Dependency injection](https://github.com/Transitio/Transitio/tree/main/docs/dependency-injection.md): assembly scanning, configurable lifetimes  
  (scoped converters), fail-fast validation, and keyed mappers  
- ✅ [Object validation](https://github.com/Transitio/Transitio/tree/main/docs/validation.md) with the standalone `Transitio.Validation` package
  (fluent rules, `ValidationResult`, DI registration)
- 📨 [Mediator](https://github.com/Transitio/Transitio/tree/main/docs/mediator.md) with the standalone `Transitio.Mediator` package 
(request/response, notifications, pipeline behaviors, DI registration)
- 🎯 Supports .NET 8 & .NET 10  

---

## 📚 Documentation

- [Getting Started](https://github.com/Transitio/Transitio/tree/main/docs/getting-started.md) – install, first map, mapping rules  
- [Core Mapping Features](https://github.com/Transitio/Transitio/tree/main/docs/mapping-features.md) – `ForMember`, `Ignore`, `Condition`, `ReverseMap`, nested & collection mapping, null handling, validation  
- [Full Object Transformation](https://github.com/Transitio/Transitio/tree/main/docs/converters.md) – `ConvertUsing` (type / instance / delegate) and `IMappingContext`  
- [Inheritance & Reusability](https://github.com/Transitio/Transitio/tree/main/docs/inheritance.md) – `Include` / `IncludeBase`  
- [Dependency Injection](https://github.com/Transitio/Transitio/tree/main/docs/dependency-injection.md) – `AddTransitio`, assembly scanning, lifetimes, validation, keyed mappers  
- [Object Validation](https://github.com/Transitio/Transitio/tree/main/docs/validation.md) - `AbstractValidator<T>`, `RuleFor`, built-in rules, `AddTransitioValidation`
- [Mediator](https://github.com/Transitio/Transitio/tree/main/docs/mediator.md) - `IRequest`/`IRequestHandler`, `INotification`/`INotificationHandler`, `IPipelineBehavior`, `AddTransitioMediator`
A runnable end-to-end demo lives in [`samples/BasicSample`](https://github.com/Transitio/Transitio/tree/main/samples/BasicSample).

See the [Changelog](https://github.com/Transitio/Transitio/blob/main/CHANGELOG.md) for release history.

---

## 👤 Maintainers

Maintained by the **Transitio**

- GitHub: https://github.com/Transitio/Transitio

---

## 📄 License

This project is licensed under the [MIT License](https://github.com/Transitio/Transitio/blob/main/LICENSE.txt)