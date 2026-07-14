# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] 2026-07-14

### Added
- **`Transitio.Mediator`** — a new standalone, in-process mediator package. Send requests to a single handler with `mediator.Send(...)`
  (`IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>`), including void requests via `IRequest` / `IRequestHandler<TRequest>`,
  and publish notifications to many handlers with `mediator.Publish(...)` (`INotification` / `INotificationHandler<TNotification>`).
- **Pipeline behaviors** — `IPipelineBehavior<TRequest, TResponse>` wraps request handling for cross-cutting concerns (logging,
  validation, transactions); behaviors run in registration order and can short-circuit or transform the response.
- **DI registration** — `AddTransitioMediator(params Assembly[])` (and a `ServiceLifetime` overload) registers `IMediator`/`ISender`/
  `IPublisher` and scans for handlers and closed pipeline behaviors (default `Transient`).
- Documentation: `docs/mediator.md`, plus a `MediatorFeaturesDemo` in `samples/BasicSample`.

### Changed
- **`Transitio.Validation` and `Transitio.Mediator`** now depend on `Microsoft.Extensions.DependencyInjection.Abstractions` instead of
  the full `Microsoft.Extensions.DependencyInjection` package. Both libraries only register and resolve services, so the lighter contracts
  package is the correct dependency. Consumers that relied on the concrete container flowing transitively (e.g. calling
  `BuildServiceProvider()` without referencing it directly) should add `Microsoft.Extensions.DependencyInjection` themselves — as most host
  applications already do.


## [1.0.8] 2026-07-13

### Changed
- Document alignment.

## [1.0.7] 2026-07-13

### Added
- **`Transitio.Validation`** — a new standalone, fluent object-validation package. Derive from `AbstractValidator<T>` and declare rules with `RuleFor(...)`; validation returns a `ValidationResult` (`IsValid`, `Errors`) and `ValidateAndThrow` raises a `ValidationException`.
- Built-in validators: `NotNull`, `NotEmpty`, `Must`, `Equal` / `NotEqual`, `Length` / `MinimumLength` / `MaximumLength`, `Matches`, `EmailAddress`, and `GreaterThan` / `GreaterThanOrEqual` / `LessThan` / `LessThanOrEqual` / `InclusiveBetween`, plus `WithMessage` / `WithErrorCode` chain modifiers.
- **DI registration** — `AddTransitioValidation(params Assembly[])` (and a `ServiceLifetime` overload) scans for `IValidator<T>` implementations with a public parameterless constructor and registers them (default `Singleton`).
- Documentation: `docs/validation.md`, plus a `ValidationFeaturesDemo` in `samples/BasicSample`.

## [1.0.6] 2026-06-16

### Added
- **Assembly scanning for profiles** — `AddTransitio(params Assembly[])` and `AddProfilesFromAssemblies(...)` / `AddProfilesFromAssemblyContaining<T>()` discover and register all `MappingProfile` types automatically.
- **Configurable service lifetime** — `AddTransitio(ServiceLifetime, ...)` registers the mapper as `Singleton` / `Scoped` / `Transient`, so a `ConvertUsing<TConverter>()` converter can depend on scoped services (e.g. a `DbContext`) without a captive dependency.
- **Fail-fast configuration validation** — opt in with `cfg.ValidateConfiguration()` to validate every map at startup (during `AddTransitio`) instead of at the first `Map` call.
- **Keyed mappers** — `AddKeyedTransitio(key, ...)` registers multiple independent mapper configurations side by side, resolvable via .NET keyed services (`GetRequiredKeyedService<IMapper>(key)` or `[FromKeyedServices]`).
- Documentation: split into a thin `README.md` plus a `docs/` folder (getting-started, mapping-features, converters, inheritance, dependency-injection), and a `DependencyFeaturesDemo` in `samples/BasicSample`.

### Changed
- `IMapper` is now resolvable and constructor-injectable directly from the container (in addition to the `TransitioDependency.Mapping.Mapper` accessor).

## [1.0.5] - 2026-06-11

### Changed
- Switched the project license to the MIT License.

## [1.0.4] - 2026-06-10

### Added
- **Full object transformation** with `ConvertUsing(...)`: type-based (`ConvertUsing<TConverter>()`), instance-based (`ConvertUsing(instance)`), and delegate-based (`ConvertUsing((src, ctx) => ...)`) converters.
- Context-aware mapping via `IMappingContext`.
- **Inheritance & reuse** with `Include<TBaseSource, TBaseDestination>()` and `IncludeBase<TBaseSource, TBaseDestination>()`, including multi-level inheritance and overriding inherited members via `ForMember`.

### Fixed
- Test suite corrections.

## [1.0.3] - 2026-06-01

### Added
- Custom member mapping with `ForMember(...).MapFrom(...)`.
- Property `Ignore()` and conditional mapping `Condition(...)`.
- Reverse mapping with `ReverseMap()`.
- `SetIgnoreNullValues(true)` to preserve destination defaults when source values are null.
- Collection mapping: `List` -> `List`, `IEnumerable` -> array, and `IEnumerable` -> Interface destinations.
- Expanded samples and tests.

## [1.0.2] - 2026-05-21

### Added
- CI/CD pipeline and release automation.

## [1.0.1] - 2026-05-21

### Added
- Initial license file.

## [1.0.0] - 2026-05-21

### Added
- Initial release of the Transitio mapping framework: `CreateMap`, profiles, name-based member mapping, nested object mapping, and `Microsoft.Extensions.DependencyInjection` integration via `Transitio.Dependency`.
[1.1.0]: https://github.com/Transitio/Transitio/compare/v1.0.8...HEAD  
[1.0.8]: https://github.com/Transitio/Transitio/compare/v1.0.7...v1.0.8  
[1.0.7]: https://github.com/Transitio/Transitio/compare/v1.0.6...v1.0.7  
[1.0.6]: https://github.com/Transitio/Transitio/compare/v1.0.5...v1.0.6  
[1.0.5]: https://github.com/Transitio/Transitio/compare/v1.0.4...v1.0.5  
[1.0.4]: https://github.com/Transitio/Transitio/compare/v1.0.2...v1.0.3  
[1.0.2]  https://github.com/Transitio/Transitio/compare/v1.0.1...v1.0.2  
[1.0.1]  https://github.com/Transitio/Transitio/compare/v1.0.0...v1.0.1  
[1.0.0]  https://github.com/Transitio/Transitio/releases/tag/v1.0.0