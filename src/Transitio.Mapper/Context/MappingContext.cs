#nullable enable
using System.Collections.Generic;

namespace Transitio.Mapper;

/// <summary>
/// Represents the context for a mapping operation.
/// Tracks object references for cycle detection and provides mapper access for recursive mapping.
/// </summary>
public class MappingContext : IMappingContext
{
    /// <summary>
    /// Gets the mapper instance that can be used for recursive mapping operations.
    /// Initialized by TransitioMapper when the mapping context is created.
    /// </summary>
    public IMapper? Mapper { get; set; }

    /// <summary>
    /// Gets the custom items dictionary for storing arbitrary context-specific data.
    /// </summary>
    public Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// Gets the object cache used for cycle detection during nested object mapping.
    /// Uses reference equality to identify circular references.
    /// </summary>
    public Dictionary<object, object> ObjectCache { get; } = new(new ReferenceEqualityComparer());

    /// <summary>
    /// Gets the list of mapping definitions available in this context.
    /// </summary>
    public List<IMappingDefinition>? Mappings { get; set; }
}