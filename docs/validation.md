# Validation

[← Back to README](../README.md) · [Getting Started](getting-started.md)

`Transitio.Validation` is a lightweight, fluent object-validation library. Declare rules for a
type, run them, and get a structured result. It is **standalone** — it does not depend on
`Transitio.Mapper` or `Transitio.Dependency`.

```bash
dotnet add package Transitio.Validation
```

## Defining a validator

Derive from `AbstractValidator<T>` and declare rules in the constructor with `RuleFor(...)`:

```csharp
using Transitio.Validation;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50);

        RuleFor(c => c.Email)
            .NotNull()
            .EmailAddress();

        RuleFor(c => c.Age)
            .InclusiveBetween(0, 150);
    }
}
```

## Running validation

```csharp
var validator = new CustomerValidator();

ValidationResult result = validator.Validate(customer);

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
        Console.WriteLine($"[{failure.ErrorCode}] {failure.PropertyName}: {failure.ErrorMessage}");
}
```

All rules run on every `Validate` call (a **Continue** cascade), so every failure is reported at
once rather than stopping at the first.

`ValidationResult` exposes:

| Member | Description |
| --- | --- |
| `IsValid` | `true` when there are no failures |
| `Errors` | `IReadOnlyList<ValidationFailure>` |
| `ToString()` | the error messages joined by newlines |

Each `ValidationFailure` carries `PropertyName`, `ErrorMessage`, `AttemptedValue`, and an optional
`ErrorCode`.

### Throwing on failure

```csharp
validator.ValidateAndThrow(customer); // throws ValidationException when invalid
```

`ValidationException.Errors` (and `.Result`) expose the failures.

## Built-in validators

| Validator | Applies to | Notes |
| --- | --- | --- |
| `NotNull()` | any | |
| `NotEmpty()` | any | empty string / whitespace, empty collection, or default value |
| `Must(predicate)` | any | `Func<TProperty,bool>` or `Func<T,TProperty,bool>` |
| `Equal(value)` / `NotEqual(value)` | any | optional `IEqualityComparer<T>` |
| `Length(min, max)` | string | |
| `MinimumLength(n)` / `MaximumLength(n)` | string | |
| `Matches(pattern)` | string | regular expression |
| `EmailAddress()` | string | |
| `GreaterThan` / `GreaterThanOrEqual` | `IComparable<T>` | |
| `LessThan` / `LessThanOrEqual` | `IComparable<T>` | |
| `InclusiveBetween(min, max)` | `IComparable<T>` | |

Value-based validators (length, format, comparison) treat a `null` value as a **skip**, so they
compose cleanly with `NotNull()` instead of producing duplicate failures.

### Customizing messages and codes

`WithMessage` and `WithErrorCode` apply to the most recently added validator in the chain:

```csharp
RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required").WithErrorCode("REQUIRED");
```

## Custom rules

Use `Must` for ad-hoc logic:

```csharp
RuleFor(c => c.Age).Must(age => age % 2 == 0).WithMessage("Age must be even");

// The instance-aware overload gives access to the whole object:
RuleFor(c => c.DiscountCode).Must((customer, code) => customer.IsPremium || code is null);
```

## Dependency injection

`AddTransitioValidation` scans assemblies for concrete `IValidator<T>` implementations that have a
**public parameterless constructor** and registers each as its closed interface (and concrete
type):

```csharp
using Transitio.Validation;

services.AddTransitioValidation(typeof(CustomerValidator).Assembly);

// Resolve and use:
var validator = provider.GetRequiredService<IValidator<Customer>>();
```

The default lifetime is `Singleton` (validators are stateless); pass a `ServiceLifetime` to
override it. Validators that need constructor dependencies should be registered manually.

## Not in this version

The following are planned for a later release: `CascadeMode.StopOnFirst`, `RuleForEach` for
collection elements, child-validator composition (`SetValidator`), asynchronous validators,
severity levels, and message localization.
