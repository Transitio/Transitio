using System.Collections.Generic;
using System.Linq;

namespace Transitio.Validation;

/// <summary>
/// The outcome of validating an instance: the set of failures, and whether it is valid.
/// </summary>
public sealed class ValidationResult
{
    private static readonly IReadOnlyList<ValidationFailure> Empty = new List<ValidationFailure>();

    /// <summary>Creates a successful (empty) result.</summary>
    public ValidationResult()
    {
        Errors = Empty;
    }

    public ValidationResult(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures?.ToList() ?? new List<ValidationFailure>();
    }

    /// <summary>All failures collected during validation. Empty when valid.</summary>
    public IReadOnlyList<ValidationFailure> Errors { get; }

    /// <summary><c>true</c> when there are no failures.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>The error messages joined by newlines (empty string when valid).</summary>
    public override string ToString()
        => string.Join("\n", Errors.Select(e => e.ErrorMessage));
}
