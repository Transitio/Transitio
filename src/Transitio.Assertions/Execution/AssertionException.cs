using System;

namespace Transitio.Assertions;

/// <summary>
/// Thrown when an assertion fails. Kept framework-agnostic so the library works under xUnit,
/// NUnit, MSTest, or any runner without taking a dependency on it.
/// </summary>
public sealed class AssertionException : Exception
{
    /// <summary>Creates a new <see cref="AssertionException"/> with the given failure message.</summary>
    public AssertionException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="AssertionException"/> with a message and inner exception.</summary>
    public AssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
