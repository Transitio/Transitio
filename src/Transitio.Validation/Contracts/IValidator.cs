namespace Transitio.Validation;

/// <summary>
/// Non-generic validator surface, useful for resolving and invoking validators without
/// knowing the closed type at the call site.
/// </summary>
public interface IValidator
{
    /// <summary>Validates the given instance and returns the result.</summary>
    ValidationResult Validate(object instance);
}

/// <summary>
/// Validates instances of <typeparamref name="T"/>.
/// </summary>
public interface IValidator<in T> : IValidator
{
    /// <summary>Validates the given instance and returns the result.</summary>
    ValidationResult Validate(T instance);
}
