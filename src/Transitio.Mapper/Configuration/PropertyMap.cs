#nullable enable
using System;
using Transitio.Mapper;

/// <summary>
/// Stores property-level mapping configuration for a specific destination property.
/// Supports custom mapping functions, conditional mapping, property ignoring, and context-aware mapping.
/// </summary>
public class PropertyMap
{
    public string DestinationProperty { get; set; } = string.Empty;

    public bool Ignore { get; set; }

    /// <summary>
    /// Simple custom mapping function without context access (backward compatible).
    /// </summary>
    public Func<object, object>? CustomMapping { get; set; }

    /// <summary>
    /// Context-aware custom mapping function that has access to the mapping context and mapper.
    /// Used when recursive mapping or context data is needed.
    /// </summary>
    public Func<object, IMappingContext, object>? CustomMappingWithContext { get; set; }

    public Func<object, bool>? Condition { get; set; }
}