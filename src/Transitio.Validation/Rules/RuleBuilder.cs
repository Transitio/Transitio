using System;

namespace Transitio.Validation;

/// <summary>
/// Concrete <see cref="IRuleBuilder{T,TProperty}"/>. Appends validators to the underlying
/// <see cref="PropertyRule{T,TProperty}"/> and remembers the last one so chain modifiers
/// (<c>WithMessage</c> / <c>WithErrorCode</c>) can target it.
/// </summary>
internal sealed class RuleBuilder<T, TProperty> : IRuleBuilder<T, TProperty>
{
    private readonly PropertyRule<T, TProperty> _rule;
    private RuleComponent<T, TProperty>? _last;

    public RuleBuilder(PropertyRule<T, TProperty> rule) => _rule = rule;

    internal IRuleBuilder<T, TProperty> AddComponent(
        Func<T, TProperty, bool> predicate,
        Func<string, TProperty, string> messageFactory,
        string? errorCode = null)
    {
        var component = new RuleComponent<T, TProperty>
        {
            Predicate = predicate,
            MessageFactory = messageFactory,
            ErrorCode = errorCode,
        };

        _rule.Components.Add(component);
        _last = component;
        return this;
    }

    public IRuleBuilder<T, TProperty> WithMessage(string message)
    {
        if (_last != null)
            _last.MessageFactory = (_, _) => message;
        return this;
    }

    public IRuleBuilder<T, TProperty> WithErrorCode(string errorCode)
    {
        if (_last != null)
            _last.ErrorCode = errorCode;
        return this;
    }
}

/// <summary>
/// Bridges built-in validator extension methods (declared over the public
/// <see cref="IRuleBuilder{T,TProperty}"/>) to the internal component pipeline.
/// </summary>
internal static class RuleBuilderInternalExtensions
{
    internal static IRuleBuilder<T, TProperty> AddComponent<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder,
        Func<T, TProperty, bool> predicate,
        Func<string, TProperty, string> messageFactory,
        string? errorCode = null)
        => ((RuleBuilder<T, TProperty>)builder).AddComponent(predicate, messageFactory, errorCode);
}
