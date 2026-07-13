using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Transitio.Validation;

/// <summary>
/// Built-in property validators. Each is an extension over <see cref="IRuleBuilder{T,TProperty}"/>
/// and appends one validator to the chain. Validators that operate on a value (length,
/// comparison, format) treat <c>null</c> as a skip so they compose cleanly with
/// <see cref="NotNull{T,TProperty}"/> instead of producing duplicate failures.
/// </summary>
public static class BuiltInValidators
{
    // — Presence

    public static IRuleBuilder<T, TProperty> NotNull<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
        => builder.AddComponent(
            (_, value) => value is not null,
            (name, _) => $"'{name}' must not be null.",
            "NotNull");

    public static IRuleBuilder<T, TProperty> NotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
        => builder.AddComponent(
            (_, value) => !IsEmpty(value),
            (name, _) => $"'{name}' must not be empty.",
            "NotEmpty");

    // — Custom predicate

    public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<TProperty, bool> predicate, string? errorMessage = null)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return builder.AddComponent(
            (_, value) => predicate(value),
            (name, _) => errorMessage ?? $"The specified condition was not met for '{name}'.",
            "Must");
    }

    public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, TProperty, bool> predicate, string? errorMessage = null)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return builder.AddComponent(
            predicate,
            (name, _) => errorMessage ?? $"The specified condition was not met for '{name}'.",
            "Must");
    }

    // — Equality

    public static IRuleBuilder<T, TProperty> Equal<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value, IEqualityComparer<TProperty>? comparer = null)
    {
        comparer ??= EqualityComparer<TProperty>.Default;
        return builder.AddComponent(
            (_, v) => comparer.Equals(v, value),
            (name, _) => $"'{name}' must equal '{value}'.",
            "Equal");
    }

    public static IRuleBuilder<T, TProperty> NotEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value, IEqualityComparer<TProperty>? comparer = null)
    {
        comparer ??= EqualityComparer<TProperty>.Default;
        return builder.AddComponent(
            (_, v) => !comparer.Equals(v, value),
            (name, _) => $"'{name}' must not equal '{value}'.",
            "NotEqual");
    }

    // — String length / format

    public static IRuleBuilder<T, string?> Length<T>(this IRuleBuilder<T, string?> builder, int min, int max)
        => builder.AddComponent(
            (_, v) => v == null || (v.Length >= min && v.Length <= max),
            (name, _) => $"'{name}' must be between {min} and {max} characters.",
            "Length");

    public static IRuleBuilder<T, string?> MinimumLength<T>(this IRuleBuilder<T, string?> builder, int min)
        => builder.AddComponent(
            (_, v) => v == null || v.Length >= min,
            (name, _) => $"'{name}' must be at least {min} characters.",
            "MinimumLength");

    public static IRuleBuilder<T, string?> MaximumLength<T>(this IRuleBuilder<T, string?> builder, int max)
        => builder.AddComponent(
            (_, v) => v == null || v.Length <= max,
            (name, _) => $"'{name}' must be {max} characters or fewer.",
            "MaximumLength");

    public static IRuleBuilder<T, string?> Matches<T>(this IRuleBuilder<T, string?> builder, string pattern)
    {
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        var regex = new Regex(pattern);
        return builder.AddComponent(
            (_, v) => v == null || regex.IsMatch(v),
            (name, _) => $"'{name}' is not in the correct format.",
            "Matches");
    }

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IRuleBuilder<T, string?> EmailAddress<T>(this IRuleBuilder<T, string?> builder)
        => builder.AddComponent(
            (_, v) => v == null || EmailRegex.IsMatch(v),
            (name, _) => $"'{name}' is not a valid email address.",
            "EmailAddress");

    // — Comparisons

    public static IRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
        => builder.AddComponent(
            (_, v) => v != null && v.CompareTo(threshold) > 0,
            (name, _) => $"'{name}' must be greater than {threshold}.",
            "GreaterThan");

    public static IRuleBuilder<T, TProperty> GreaterThanOrEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
        => builder.AddComponent(
            (_, v) => v != null && v.CompareTo(threshold) >= 0,
            (name, _) => $"'{name}' must be greater than or equal to {threshold}.",
            "GreaterThanOrEqual");

    public static IRuleBuilder<T, TProperty> LessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
        => builder.AddComponent(
            (_, v) => v != null && v.CompareTo(threshold) < 0,
            (name, _) => $"'{name}' must be less than {threshold}.",
            "LessThan");

    public static IRuleBuilder<T, TProperty> LessThanOrEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty threshold)
        where TProperty : IComparable<TProperty>
        => builder.AddComponent(
            (_, v) => v != null && v.CompareTo(threshold) <= 0,
            (name, _) => $"'{name}' must be less than or equal to {threshold}.",
            "LessThanOrEqual");

    public static IRuleBuilder<T, TProperty> InclusiveBetween<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty min, TProperty max)
        where TProperty : IComparable<TProperty>
        => builder.AddComponent(
            (_, v) => v != null && v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0,
            (name, _) => $"'{name}' must be between {min} and {max}.",
            "InclusiveBetween");

    // — Helpers

    private static bool IsEmpty<TProperty>(TProperty value)
    {
        switch (value)
        {
            case null:
                return true;
            case string s:
                return string.IsNullOrWhiteSpace(s);
            case IEnumerable enumerable:
                var enumerator = enumerable.GetEnumerator();
                using (enumerator as IDisposable)
                    return !enumerator.MoveNext();
            default:
                return EqualityComparer<TProperty>.Default.Equals(value, default!);
        }
    }
}
