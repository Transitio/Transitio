# Transitio

[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](https://www.nuget.org/packages/Transitio.Mapper)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](https://www.nuget.org/packages/Transitio.Dependency)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Validation)](https://www.nuget.org/packages/Transitio.Validation)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Mediator)](https://www.nuget.org/packages/Transitio.Mediator)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Assertions)](https://www.nuget.org/packages/Transitio.Assertions)

Lightweight, high-performance .NET libraries for object mapping, dependency injection, validation, and an in-process mediator. Targets .NET 8 & .NET 10.

## Install

```bash
dotnet add package Transitio.Mapper       # object mapping
dotnet add package Transitio.Dependency   # DI integration
dotnet add package Transitio.Validation   # object validation
dotnet add package Transitio.Mediator     # in-process mediator
dotnet add package Transitio.Assertions   # fluent test assertions
```

## Quick start

```csharp
using Transitio.Mapper;

var config = new TransitioMapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
IMapper mapper = config.BuildMapper();

var dto = mapper.Map<UserDto>(user);
```

Using DI? Register with `services.AddTransitio(...)` and inject `IMapper`.

## Documentation

Full docs live in the **[project wiki](https://github.com/Transitio/Transitio/wiki)**:

- [Getting Started](https://github.com/Transitio/Transitio/wiki/Getting-Started)
- [Transitio.Mapper](https://github.com/Transitio/Transitio/wiki/Transitio.Mapper)
- [Transitio.Dependency](https://github.com/Transitio/Transitio/wiki/Transitio.Dependency)
- [Transitio.Validation](https://github.com/Transitio/Transitio/wiki/Transitio.Validation)
- [Transitio.Mediator](https://github.com/Transitio/Transitio/wiki/Transitio.Mediator)
- [Transitio.Assertions](https://github.com/Transitio/Transitio/wiki/Transitio.Assertions)
- [FAQ](https://github.com/Transitio/Transitio/wiki/FAQ)
- [Roadmap](https://github.com/Transitio/Transitio/wiki/Roadmap)
- [Contributing](https://github.com/Transitio/Transitio/wiki/Contributing)

Runnable demo: [`samples/BasicSample`](https://github.com/Transitio/Transitio/tree/main/samples/BasicSample). 
See the [Changelog](https://github.com/Transitio/Transitio/blob/main/CHANGELOG.md) for release history.

## 👤 Maintainers

Maintained by the **Transitio**

- GitHub: https://github.com/Transitio/Transitio

## 📄 License

This project is licensed under the [MIT License](https://github.com/Transitio/Transitio/blob/main/LICENSE.txt)
