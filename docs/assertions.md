# Assertions

[← Back to README](../README.md) · [Getting Started](getting-started.md)

`Transitio.Assertions` is a lightweight, fluent assertion library for tests. Call `.Should()` on any
value to start an assertion chain that reads like a sentence and fails with a clear, readable message.
It is **standalone and dependency-free** — it throws its own `AssertionException`, so it works under
xUnit, NUnit, or MSTest without referencing any of them.

```bash
dotnet add package Transitio.Assertions
```

```csharp
using Transitio.Assertions;

result.Should().Be(expected);
name.Should().NotBeNull().And.StartWith("Tra");
```

Every assertion returns an `AndConstraint<T>`, so you can chain with `.And`, and every method takes an
optional `because` reason (with `string.Format` args) that is appended to the failure message.

```csharp
order.Total.Should().BeGreaterThan(0, "an order always has a positive total");
// AssertionException: Expected value to be greater than 0 because an order always has a positive total, but found ...
```

## Objects — equality, nullability, type

```csharp
customer.Should().NotBeNull();
result.Should().Be(expected).And.NotBe(other);

response.Should().BeOfType<OkResult>();          // exact type
handler.Should().BeAssignableTo<IHandler>();     // implements / derives
cached.Should().BeSameAs(original);              // reference identity
```

`Be`/`NotBe` use the default equality (`object.Equals`), so records and structs compare by value.

## Booleans

```csharp
isActive.Should().BeTrue();
hasErrors.Should().BeFalse("the payload was valid");
```

## Strings

```csharp
slug.Should().Be("order-123").And.HaveLength(9);
message.Should().Contain("saved").And.StartWith("Order").And.EndWith("saved");
name.Should().NotBeNullOrWhiteSpace();
id.Should().Match("order-*");    // '*' = any run of characters, '?' = a single character
```

## Numbers

Numeric types (`int`, `uint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `double`, `float`,
`decimal`) get ordering, range, sign and tolerance assertions.

```csharp
age.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(120);
total.Should().BeInRange(1m, 100m);
balance.Should().BePositive();
delta.Should().BeNegative();
```

Never compare floating-point values with `Be` — use `BeApproximately` with a tolerance:

```csharp
(0.1 + 0.2).Should().BeApproximately(0.3, 1e-9);
average.Should().BeApproximately(4.2, 0.05);
```

## Dates, durations and other comparables

`DateTime`, `DateTimeOffset`, `TimeSpan`, `char`, and any other `struct` that is `IComparable<T>` get
the ordering and range assertions (sign and tolerance checks are numeric-only).

```csharp
createdAt.Should().BeGreaterThan(new DateTime(2026, 1, 1));
window.Should().BeInRange(TimeSpan.Zero, TimeSpan.FromHours(1));
grade.Should().BeGreaterThanOrEqualTo('B');
```

## Collections

The sequence is enumerated once, so lazy queries and single-pass iterators are safe to assert on.

```csharp
orders.Should().NotBeEmpty().And.HaveCount(3);
names.Should().Contain("Ada").And.Contain(n => n.StartsWith("Al"));
ages.Should().OnlyContain(a => a >= 0);
result.Should().ContainSingle();
result.Should().BeEquivalentTo(new[] { 3, 2, 1 });  // same elements, any order
```

## Exceptions

```csharp
Action act = () => service.Withdraw(-1);
act.Should().Throw<ArgumentException>()
    .WithMessage("*amount*")
    .Which.ParamName.Should().Be("amount");

act.Should().Throw<InvalidOperationException>()
    .WithInnerException<TimeoutException>();

Action ok = () => service.Ping();
ok.Should().NotThrow();
```

Async delegates are supported too:

```csharp
Func<Task> act = () => service.SaveAsync(null);
await act.Should().ThrowAsync<ArgumentNullException>();

Func<Task> ok = () => service.SaveAsync(entity);
await ok.Should().NotThrowAsync();
```

`Throw`/`ThrowAsync` return an `ExceptionAssertions<T>` whose `.Which` exposes the captured exception for
further assertions.
