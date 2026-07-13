using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Transitio.Validation;

/// <summary>A single configured validator attached to a property.</summary>
internal sealed class RuleComponent<T, TProperty>
{
    public required Func<T, TProperty, bool> Predicate { get; init; }

    /// <summary>
    /// Builds the failure message from (propertyName, value). Mutable so
    /// <c>WithMessage</c> can replace it.
    /// </summary>
    public Func<string, TProperty, string> MessageFactory { get; set; } = (name, _) => $"'{name}' is invalid.";

    public string? ErrorCode { get; set; }
}

/// <summary>Non-generic view so <see cref="AbstractValidator{T}"/> can run rules uniformly.</summary>
internal interface IPropertyRule<in T>
{
    IEnumerable<ValidationFailure> Validate(T instance);
}

internal sealed class PropertyRule<T, TProperty> : IPropertyRule<T>
{
    private readonly Func<T, TProperty> _accessor;

    public PropertyRule(Expression<Func<T, TProperty>> expression)
    {
        PropertyName = ExpressionHelper.GetMemberName(expression);
        _accessor = expression.Compile();
    }

    public string PropertyName { get; }

    public List<RuleComponent<T, TProperty>> Components { get; } = new();

    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        var value = _accessor(instance);

        foreach (var component in Components)
        {
            if (!component.Predicate(instance, value))
            {
                yield return new ValidationFailure(
                    PropertyName,
                    component.MessageFactory(PropertyName, value),
                    value,
                    component.ErrorCode);
            }
        }
    }
}
