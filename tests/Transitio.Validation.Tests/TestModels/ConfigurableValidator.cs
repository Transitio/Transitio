using System;
using System.Linq.Expressions;

namespace Transitio.Validation.Tests;

/// <summary>
/// Test helper for exercising individual rules inline. Its constructor takes an
/// <see cref="Action{T}"/>, so it has no parameterless constructor and is therefore ignored by
/// <c>AddTransitioValidation</c>'s assembly scan — keeping DI tests deterministic.
/// </summary>
public sealed class ConfigurableValidator : AbstractValidator<Customer>
{
    public ConfigurableValidator(Action<ConfigurableValidator> configure) => configure(this);

    public IRuleBuilder<Customer, TProperty> For<TProperty>(Expression<Func<Customer, TProperty>> expression)
        => RuleFor(expression);
}
