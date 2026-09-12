# Roadmap
 
This roadmap is prioritized by implementation effort and risk of surprising behavior ("footguns"), not by user-reported demand — there are currently no open issues or discussions requesting these features. Items are pulled from a gap analysis against the well-known libraries each Transitio package mirrors (AutoMapper, FluentValidation, MediatR, FluentAssertions, and common DI-container conventions).
 
Dates are targets, not commitments, and assume a similar release cadence to recent history (see [CHANGELOG](Transitio/CHANGELOG.md at main · Transitio/Transitio)).
 
## Milestone 1 — v1.3.0 "Quick wins" (target: 2026-08-29)
 
Low effort, no breaking changes, each closes a real gap in current behavior.
 
- **Transitio.Mediator** — Auto-register open-generic pipeline behaviors. Today, a behavior like `LoggingBehavior<,>` registered via assembly scanning is silently skipped by `AddTransitioMediator`, with no error.
- **Transitio.Validation** — `CascadeMode.StopOnFirstFailure`, so a single bad property doesn't report every subsequent rule failure on it.
- **Transitio.Mapper** — `Map(source, destination)` overload to map into an existing instance (e.g. updating an entity from a DTO).
- **Transitio.Dependency** — `TryAddTransitio` for idempotent registration in composed applications.
- **Transitio.Assertions** — `ContainKey`/`ContainValue` for dictionaries, `Guid` support.
 
## Milestone 2 — v1.4.0 "Conditional & lifecycle" (target: 2026-09-19)
 
Moderate effort, still purely additive.
 
- **Transitio.Validation** — `When`/`Unless` conditional rules, `RuleForEach` for collection elements.
- **Transitio.Mapper** — `BeforeMap`/`AfterMap` hooks, `NullSubstitute` and null-collection handling.
- **Transitio.Mediator** — Configurable parallel notification publishing.
- **Transitio.Assertions** — `Should().Match(predicate)`, `BeCloseTo` for dates.
 
## Milestone 3 — v1.5.0 "Structural composition" (target: 2026-10-17)
 
Larger design surface — nesting/composition semantics that need care to avoid future breaking changes.
 
- **Transitio.Validation** — `SetValidator` for nested/child validator composition.
- **Transitio.Mapper** — Flattening convention (dotted member matching, e.g. `Customer.Name` → `CustomerName`); reusable `IValueResolver<TSource,TDest,TMember>` classes.
- **Transitio.Dependency** — `Decorate<TService,TDecorator>()`; `ValidateOnStart` hosted-service hook.
- **Transitio.Assertions** — Collection ordering/subset assertions (`BeInAscendingOrder`, `BeSubsetOf`).
 
## Milestone 4 — v2.0.0-track "Major investments" (target: Q4 2026)
 
High effort and/or new API shapes. Each of these is substantial enough to warrant its own design note and release rather than being bundled together.
 
- **Transitio.Validation** — Async validation (`MustAsync`, `ValidateAsync`).
- **Transitio.Mediator** — Streaming requests (`IStreamRequest<T>` + `IAsyncEnumerable<T>`).
- **Transitio.Mapper** — `ProjectTo<T>()` over `IQueryable` for EF-friendly server-side projection.
- **Transitio.Assertions** — Recursive object `BeEquivalentTo` (deep graph comparison).
- **Transitio.Mapper** — Constructor-based mapping for destination types without a parameterless constructor.
 
---
 
Have a feature request that isn't listed here? Open an issue or start a discussion — this roadmap currently reflects a gap analysis, not community input, so real requests will get reprioritized above it.