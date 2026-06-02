using System;
using Transitio.Mapper;

/// <summary>
/// Configuration options for mapping individual members within a ForMember call.
/// Supports property-level mapping customization including custom mapping functions,
/// conditional mapping, and property ignoring.
/// </summary>
public class MemberOptions<TSource>
{
    private readonly PropertyMap _propertyMap;

    public MemberOptions(PropertyMap propertyMap)
    {
        _propertyMap = propertyMap;
    }

    /// <summary>
    /// Specifies a custom mapping function for this property.
    /// Legacy overload for backward compatibility - source value only.
    /// </summary>
    /// <param name="mapFunc">Function that maps from source value to destination value</param>
    public void MapFrom(Func<TSource, object> mapFunc)
    {
        _propertyMap.CustomMapping = src => mapFunc((TSource)src);
        _propertyMap.CustomMappingWithContext = null;  // Clear context-aware version
    }

    /// <summary>
    /// Specifies a custom mapping function for this property with access to the mapping context.
    /// Allows recursive mapping and access to the mapper instance.
    /// </summary>
    /// <param name="mapFunc">Function that maps from source value and context to destination value</param>
    public void MapFrom(Func<TSource, IMappingContext, object> mapFunc)
    {
        _propertyMap.CustomMappingWithContext = (src, ctx) => mapFunc((TSource)src, ctx);
        _propertyMap.CustomMapping = null;  // Clear simple version
    }

    /// <summary>
    /// Marks this property to be ignored during mapping.
    /// The property will retain its default value.
    /// </summary>
    public void Ignore()
    {
        _propertyMap.Ignore = true;
    }

    /// <summary>
    /// Specifies a condition that determines whether this property should be mapped.
    /// If the condition evaluates to false, the property retains its default value.
    /// </summary>
    /// <param name="condition">Predicate that returns true if the property should be mapped</param>
    public void Condition(Func<TSource, bool> condition)
    {
        _propertyMap.Condition = src => condition((TSource)src);
    }
}
