# Full Object Transformation — `ConvertUsing(...)`

[← Back to README](../README.md) · [Getting Started](getting-started.md)

`ConvertUsing` replaces the default property-by-property mapping with your own conversion logic. It comes in three flavours.

## Type-based converter — `ConvertUsing<TConverter>()`

```csharp
public class OrderConverter : ITypeConverter<OrderInput, OrderDomain>
{
    public OrderDomain Convert(OrderInput source, IMappingContext context)
        => new OrderDomain
        {
            FinalAmount = source.Amount,
            Region = source.Country == "IN" ? "APAC" : "GLOBAL"
        };
}

cfg.CreateMap<OrderInput, OrderDomain>().ConvertUsing<OrderConverter>();
```

When registered through `Transito.Dependency`, type-based converters are resolved from the container, so they may declare constructor dependencies. See [Configurable lifetime](dependency-injection.md#configurable-lifetime-scoped-converters).

## Instance-based converter — `ConvertUsing(instance)`

```csharp
var converter = new PricingConverter(new PricingService());
cfg.CreateMap<OrderInput, OrderDomain>().ConvertUsing(converter);
```

## Delegate-based converter — `ConvertUsing((src, ctx) => ...)`

The delegate receives the source and an `IMappingContext` for context-aware mapping:

```csharp
cfg.CreateMap<OrderInput, OrderDomain>()
    .ConvertUsing((src, ctx) =>
    {
        var region = src.Country == "IN" ? "APAC" : "GLOBAL";

        // IMappingContext can carry runtime values into the conversion
        if (ctx.Items.TryGetValue("RegionOverride", out var value))
            region = value?.ToString() ?? region;

        return new OrderDomain { FinalAmount = src.Amount, Region = region };
    });
```

## `IMappingContext`

The context passed to every converter exposes:

- `Mapper` — the active `IMapper`, for recursive/nested mapping from inside a converter
- `Items` — a `Dictionary<string, object>` for arbitrary per-operation data
- `ObjectCache` — used internally for cycle detection during nested mapping