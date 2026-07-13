namespace Transitio.Validation;

/// <summary>
/// A single validation error: which property failed, why, and the value that was rejected.
/// </summary>
public sealed class ValidationFailure
{
    public ValidationFailure(string propertyName, string errorMessage, object? attemptedValue = null, string? errorCode = null)
    {
        PropertyName = propertyName ?? string.Empty;
        ErrorMessage = errorMessage ?? string.Empty;
        AttemptedValue = attemptedValue;
        ErrorCode = errorCode;
    }

    /// <summary>Name of the property that failed validation.</summary>
    public string PropertyName { get; }

    /// <summary>Human-readable description of the failure.</summary>
    public string ErrorMessage { get; }

    /// <summary>The property value that was rejected.</summary>
    public object? AttemptedValue { get; }

    /// <summary>Optional machine-readable code identifying the rule that failed.</summary>
    public string? ErrorCode { get; }

    public override string ToString() => ErrorMessage;
}
