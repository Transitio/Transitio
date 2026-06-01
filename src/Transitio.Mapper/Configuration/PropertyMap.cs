#nullable enable
using System;

public class PropertyMap
{
    public string DestinationProperty { get; set; } = string.Empty;

    public bool Ignore { get; set; }

    public Func<object, object>? CustomMapping { get; set; }

    public Func<object, bool>? Condition { get; set; }
}