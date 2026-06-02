#nullable enable
using System.Collections.Generic;

namespace Transitio.Mapper;

/// <summary>
/// Represents the context for a mapping operation.
/// Provides access to the mapper instance for recursive mapping and object caching for cycle detection.
/// </summary>
public interface IMappingContext
{
    /// <summary>
    /// Gets the mapper instance that can be used for recursive mapping operations.
    /// </summary>
    IMapper? Mapper { get; }

    /// <summary>
    /// Gets the object cache used for cycle detection during nested object mapping.
    /// Uses reference equality to identify circular references.
    /// </summary>
    Dictionary<object, object> ObjectCache { get; }

    /// <summary>
    /// Gets the custom items dictionary for storing arbitrary context-specific data.
    /// </summary>
    Dictionary<string, object> Items { get; }
}
