# Inheritance & Reusability – `Include` / `IncludeBase`

[← Back to README](../README.md) · [Getting Started](getting-started.md)

Reuse base-type mappings from derived types. Derived `ForMember` configurations override the inherited ones, and inheritance can be chained across multiple levels.

```csharp
// Base
cfg.CreateMap<Person, PersonDto>();

// Derived reuses the base mapping
cfg.CreateMap<Employee, EmployeeDto>()
    .IncludeBase<Person, PersonDto>();

// Multi-level: Manager -> Employee -> Person
cfg.CreateMap<Manager, ManagerDto>()
    .IncludeBase<Employee, EmployeeDto>();
```

`Include<TBaseSource, TBaseDestination>()` and `IncludeBase<TBaseSource, TBaseDestination>()`
are equivalent; use whichever reads better at the call site. Circular includes are detected and rejected.

## Overriding inherited members

A derived map can override an inherited member by configuring it with
[`ForMember`](mapping-features.md#custom-member-mapping--formembermapfrom):

```csharp
cfg.CreateMap<Person, PersonDto>();

cfg.CreateMap<Employee, EmployeeDto>()
    .IncludeBase<Person, PersonDto>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToUpper())); // override
```
