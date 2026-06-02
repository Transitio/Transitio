#nullable enable
using System;
using System.Collections.Generic;

/// <summary>
/// Stores type-level mapping metadata including property mappings, custom converters, and included base type mappings.
/// </summary>
public class TypeMap
{
    public Type SourceType { get; set; } = typeof(object);
    public Type DestinationType { get; set; } = typeof(object);

    public Dictionary<string, PropertyMap> PropertyMaps { get; } = new();

    /// <summary>
    /// List of included type mappings (from base/parent types).
    /// When resolving mappings, included mappings are applied first, then derived mappings override.
    /// </summary>
    public List<(Type SourceType, Type DestinationType)> IncludedMaps { get; } = new();

    /// <summary>
    /// Custom type converter instance. When set, replaces property-by-property mapping.
    /// </summary>
    public object? ConverterInstance { get; set; }

    /// <summary>
    /// Type of a custom type converter. When set, an instance will be created and used.
    /// </summary>
    public Type? ConverterType { get; set; }

    /// <summary>
    /// Custom delegate-based converter. When set, replaces property-by-property mapping.
    /// Stored as (source, context) => destination.
    /// </summary>
    public Delegate? ConverterDelegate { get; set; }
}