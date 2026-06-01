using System;
using System.Collections.Generic;

public class TypeMap
{
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public Dictionary<string, PropertyMap> PropertyMaps { get; } = new();
}