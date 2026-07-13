using System;
using System.Collections.Generic;

namespace Transitio.Validation;

/// <summary>
/// Thrown by <c>ValidateAndThrow</c> when an instance fails validation. Carries the full
/// <see cref="ValidationResult"/> so callers can inspect every failure.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(ValidationResult result)
        : base(BuildMessage(result))
    {
        Result = result;
    }

    /// <summary>The validation result that triggered this exception.</summary>
    public ValidationResult Result { get; }

    /// <summary>The failures that caused validation to fail.</summary>
    public IReadOnlyList<ValidationFailure> Errors => Result.Errors;

    private static string BuildMessage(ValidationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        return "Validation failed:\n" + result;
    }
}
