# Transitio
 
[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](NuGet Gallery | Transitio.Mapper 1.3.0)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](NuGet Gallery | Transitio.Dependency 1.3.0)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Validation)](NuGet Gallery | Transitio.Validation 1.3.0)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Mediator)](NuGet Gallery | Transitio.Mediator 1.3.0)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Assertions)](NuGet Gallery | Transitio.Assertions 1.3.0)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Logging)](https://www.nuget.org/packages/Transitio.Logging)
 
Lightweight, high-performance .NET libraries for object mapping, dependency injection, validation, an in-process mediator, and structured logging context. Targets .NET 8 & .NET 10.
 
## Install
 
```bash
dotnet add package Transitio.Mapper       # object mapping
dotnet add package Transitio.Dependency   # DI integration
dotnet add package Transitio.Validation   # object validation
dotnet add package Transitio.Mediator     # in-process mediator
dotnet add package Transitio.Assertions   # fluent test assertions
dotnet add package Transitio.Logging      # structured, correlation-aware logging context
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
- [Transitio.Dependency](Transitio.Dependency)
- [Transitio.Validation](Transitio.Validation)
- [Transitio.Mediator](https://github.com/Transitio/Transitio/wiki/Transitio.Mediator)
- [Transitio.Assertions](Transitio.Assertions)
- [Transitio.Logging](https://github.com/Transitio/Transitio/wiki/Transitio.Logging)
- [FAQ](https://github.com/Transitio/Transitio/wiki/FAQ)
- [Roadmap](https://github.com/Transitio/Transitio/wiki/Roadmap)
- [Contributing](https://github.com/Transitio/Transitio/wiki/Contributing)
 
Runnable demo: [`samples/BasicSample`](Transitio/samples/BasicSample at main · Transitio/Transitio).
See the [Changelog](Transitio/CHANGELOG.md at main · Transitio/Transitio) for release history.
 
## 👤 Maintainers
 
Maintained by the **Transitio**
 
- GitHub: https://github.com/Transitio/Transitio
 
## 📄 License
 
This project is licensed under the [MIT License](Transitio/LICENSE.txt at main · Transitio/Transitio)