# Transitio

[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](https://www.nuget.org/packages/Transitio.Mapper)  
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](https://www.nuget.org/packages/Transitio.Dependency)

**Transitio** is a lightweight, high-performance object mapping framework for .NET with support for profiles, nested mapping, and dependency injection.

---

## 📦 Installation

```bash
dotnet add package Transitio.Mapper
# optional: Microsoft.Extensions.DependencyInjection integration
dotnet add package Transitio.Dependency
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
See the [Getting Started](docs/getting-started.md) guide.

---

## ✨ Features

- ⚡ Expression-based high-performance mapping  
- 🧩 [Profiles](docs/mapping-features.md#profiles) for modular configuration  
- 🔗 [Nested object mapping](docs/mapping-features.md#nested--collection-mapping) (including nested collection properties)  
- 📦 [Collection mapping](docs/mapping-features.md#nested--collection-mapping) (List → List, IEnumerable → array, IEnumerable → interface)  
- 🧵 Thread-safe: a single mapper instance can be shared as a singleton  
- 🔧 [Custom member mapping](docs/mapping-features.md#custom-member-mapping--formembermapfrom),  
  [Ignore](docs/mapping-features.md#ignore-a-property--formemberignore),  
  [Conditional mapping](docs/mapping-features.md#conditional-mapping--formembercondition),  
  and [Reverse mapping](docs/mapping-features.md#reverse-mapping--reversemap)  
- 🔁 [Full object transformation](docs/converters.md) with `ConvertUsing(...)` (type / instance / delegate)  
  and context-aware mapping via `IMappingContext`  
- 🧬 [Inheritance & reuse](docs/inheritance.md) with `Include` / `IncludeBase`  
- 🧪 [Null-value handling](docs/mapping-features.md#preserve-destination-defaults--setignorenullvaluestrue)  
  and [configuration validation](docs/mapping-features.md#configuration-validation--assertconfigurationisvalid)  
- 📦 [Dependency injection](docs/dependency-injection.md): assembly scanning, configurable lifetimes  
  (scoped converters), fail-fast validation, and keyed mappers  
- ✅ [Object validation](docs/validation.md) with the standalone `Transitio.Validation` package
  (fluent rules, `ValidationResult`, DI registration)
- 🎯 Supports .NET 8 & .NET 10  

---

## 📚 Documentation

- [Getting Started](docs/getting-started.md) – install, first map, mapping rules  
- [Core Mapping Features](docs/mapping-features.md) – `ForMember`, `Ignore`, `Condition`, `ReverseMap`, nested & collection mapping, null handling, validation  
- [Full Object Transformation](docs/converters.md) – `ConvertUsing` (type / instance / delegate) and `IMappingContext`  
- [Inheritance & Reusability](docs/inheritance.md) – `Include` / `IncludeBase`  
- [Dependency Injection](docs/dependency-injection.md) – `AddTransitio`, assembly scanning, lifetimes, validation, keyed mappers  
- [Object Validation](docs/validation.md) - `AbstractValidator<T>`, `RuleFor`, built-in rules, `AddTransitioValidtion`
A runnable end-to-end demo lives in [`samples/BasicSample`](samples/BasicSample).

See the [Changelog](CHANGELOG.md) for release history.

---

## 👤 Maintainers

Maintained by the **Transitio**

- GitHub: https://github.com/Transitio/Transitio

---

## 📄 License

This project is licensed under the [MIT License](LICENSE.txt)