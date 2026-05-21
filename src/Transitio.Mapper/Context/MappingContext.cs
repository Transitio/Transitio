using System.Collections.Generic;

namespace Transitio.Mapper;

public class MappingContext
{
    public Dictionary<string, object> Items { get; } = new();

    // ✅ ADD THIS
    public List<IMappingDefinition>? Mappings { get; set; }
}