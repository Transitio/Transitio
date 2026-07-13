namespace Transitio.Validation;

/// <summary>
/// Fluent surface returned by <c>RuleFor</c>. Built-in validators (NotNull, Length, ...) are
/// extension methods over this interface; <see cref="WithMessage"/> and
/// <see cref="WithErrorCode"/> customize the most recently added validator in the chain.
/// </summary>
public interface IRuleBuilder<T, TProperty>
{
    /// <summary>Overrides the error message of the most recently added validator.</summary>
    IRuleBuilder<T, TProperty> WithMessage(string message);

    /// <summary>Sets the error code of the most recently added validator.</summary>
    IRuleBuilder<T, TProperty> WithErrorCode(string errorCode);
}
