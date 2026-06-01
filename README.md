# 🚀 Transitio

[![NuGet](https://img.shields.io/nuget/v/Transitio.Mapper)](https://www.nuget.org/packages/Transitio.Mapper)
[![NuGet](https://img.shields.io/nuget/v/Transitio.Dependency)](https://www.nuget.org/packages/Transitio.Dependency)

**Transitio** is a lightweight, high-performance object mapping framework for .NET with support for profiles, nested mapping, and dependency injection.

---

## ✨ Features

- ⚡ Expression-based high-performance mapping
- 🧩 Profiles for modular configuration
- 🔗 Nested object mapping
- 📦 Collection mapping (List → List, IEnumerable → array, IEnumerable → interface)
- 🔧 Custom mapping support with `ForMember(...).MapFrom(...)`
- 🚫 Property ignore support with `ForMember(...).Ignore()`
- 🎯 Conditional property mapping with `ForMember(...).Condition(...)`
- 🔄 Reverse mapping with `ReverseMap()`
- 🧪 Optional `SetIgnoreNullValues(true)` to preserve destination defaults when source properties are null
- ✅ Mapping validation
- 💉 Dependency injection integration
- 🎯 Supports .NET 8 & .NET 10

---

## 🆕 What’s new

- Added `ForMember(...).MapFrom(...)` custom member mapping
- Added `ForMember(...).Ignore()` property ignore support
- Added `ForMember(...).Condition(...)` conditional member mapping
- Added `ReverseMap()` support for reverse mappings
- Added `SetIgnoreNullValues(true)` to preserve destination defaults when source values are null
- Expanded collection mapping to support array and interface destinations

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