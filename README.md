# Transitio

[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](https://www.nuget.org/packages/Transitio.Mapper)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](https://www.nuget.org/packages/Transitio.Dependency)

**Transitio** is a lightweight, high-performance object mapping framework for .NET with support for profiles, nested mapping, and dependency injection.

---

## ✨ Features

- ⚡ Expression-based high-performance mapping
- 🧩 Profiles for modular configuration
- 🔗 Nested object mapping
- 📦 Collection mapping (List → List, IEnumerable → array, IEnumerable → interface)

### 🎯 Custom Mapping Capabilities
- 🔧 Custom member mapping with `ForMember(...).MapFrom(...)`
- 🚫 Property ignore support with `ForMember(...).Ignore()`
- 🎯 Conditional property mapping with `ForMember(...).Condition(...)`
- 🔄 Reverse mapping with `ReverseMap()`

### 🔁 Full Object Transformation
- 🔄 Replace entire mapping logic using `ConvertUsing(...)`
  - ✅ Type-based converter (`ConvertUsing<TConverter>()`)
  - ✅ Instance-based converter (`ConvertUsing(instance)`)
  - ✅ Delegate-based converter (`ConvertUsing((src, ctx) => {...})`)
- 🧠 Context-aware mapping with `IMappingContext` for runtime customization

### 🧬 Inheritance & Reusability
- 🧬 Reuse base mappings using `Include<TBaseSource, TBaseDestination>()`
- 🧬 Explicit base mapping support with `IncludeBase<TBaseSource, TBaseDestination>()`
- 🔁 Override base mappings in derived types using `ForMember`

### 🧪 Behavior Control
- 🧪 Optional `SetIgnoreNullValues(true)` to preserve destination defaults when source properties are null
- ✅ Mapping validation for early error detection

### ⚙️ Integration & Platform
- 💉 Built-in dependency injection support (`Microsoft.Extensions.DependencyInjection`)
- 🎯 Supports .NET 8 & .NET 10

---

## 🆕 What’s New

- 🔄 Introduced `ConvertUsing(...)` for full object transformation
  - Supports type-based, instance-based, and delegate-based converters
  - Enables context-aware mapping via `IMappingContext`

- 🧬 Added inheritance-based mapping support
  - `Include<TBaseSource, TBaseDestination>()`
  - `IncludeBase<TBaseSource, TBaseDestination>()`
  - Allows reuse and override of base mappings

- 🧪 Added `SetIgnoreNullValues(true)` to preserve destination defaults when source values are null

- 📦 Expanded collection mapping to support arrays and interface-based destinations

---

## 📦 Installation

bash
dotnet add package Transitio.Mapper
dotnet add package Transitio.Dependency

## 👨‍💻 Maintainers

Maintained by the **Transitio Team**

- GitHub: https://github.com/Transitio/Transitio

## 📄 License

This software is provided under a custom proprietary license.
Commercial licensing options may be available in the future.