using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Transitio.Validation;

/// <summary>
/// Base class for type validators. Derive from it and declare rules in the constructor with
/// <see cref="RuleFor{TProperty}"/>:
/// <code>
/// public class CustomerValidator : AbstractValidator&lt;Customer&gt;
/// {
///     public CustomerValidator()
///     {
///         RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
///         RuleFor(c => c.Age).GreaterThanOrEqual(0);
///     }
/// }
/// </code>
/// All rules run on every <see cref="Validate(T)"/> call (Continue cascade) so every failure
/// is reported at once.
/// </summary>
public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IPropertyRule<T>> _rules = new();

    /// <summary>Begins a rule chain for the property selected by <paramref name="expression"/>.</summary>
    protected IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var rule = new PropertyRule<T, TProperty>(expression);
        _rules.Add(rule);
        return new RuleBuilder<T, TProperty>(rule);
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var failures = new List<ValidationFailure>();
        foreach (var rule in _rules)
            failures.AddRange(rule.Validate(instance));

        return new ValidationResult(failures);
    }

    ValidationResult IValidator.Validate(object instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        if (instance is not T typed)
            throw new ArgumentException(
                $"Expected an instance of '{typeof(T)}' but received '{instance.GetType()}'.",
                nameof(instance));

        return Validate(typed);
    }

    /// <summary>Validates and throws a <see cref="ValidationException"/> if invalid.</summary>
    public void ValidateAndThrow(T instance)
    {
        var result = Validate(instance);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
