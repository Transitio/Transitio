using System;

namespace Transitio.Mediator;

/// <summary>
/// Represents a void response. Requests that produce no value (<see cref="IRequest"/>) use
/// <see cref="Unit"/> as their response type so a single dispatch pipeline can serve both
/// value-returning and void requests.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>The single <see cref="Unit"/> value.</summary>
    public static readonly Unit Value = new();

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public override string ToString() => "()";

    /// <summary>All <see cref="Unit"/> values are equal.</summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>All <see cref="Unit"/> values are equal.</summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
