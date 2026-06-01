#nullable enable
using System.Collections.Generic;

namespace Transitio.Mapper;

public class MappingContext
{
    public Dictionary<string, object> Items { get; } = new();

    public Dictionary<object, object> ObjectCache { get; } = new(new ReferenceEqualityComparer());

    public List<IMappingDefinition>? Mappings { get; set; }
}