using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Transitio.Mapper;

public class TransitioMapper : IMapper
{
    private readonly List<IMappingDefinition> _mappings;

    // ✅ NEW: TypeMaps (for ForMember, Ignore, etc.)
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps;

    private readonly bool _ignoreNullValues;

    // ✅ CACHE (key = (source, destination))
    private readonly ConcurrentDictionary<(System.Type, System.Type), Func<object, object>> _cache = new();

    public TransitioMapper(
        List<IMappingDefinition> mappings,
        Dictionary<(Type, Type), TypeMap> typeMaps,
        bool ignoreNullValues)
    {
        _mappings = mappings;
        _typeMaps = typeMaps;
        _ignoreNullValues = ignoreNullValues;
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var sourceType = source.GetType();
        var destType = typeof(TDestination);

        // ✅ Collection support
        if (IsCollection(sourceType, destType))
        {
            return (TDestination)MapCollection(source, sourceType, destType);
        }

        var key = (sourceType, destType);

        var compiled = _cache.GetOrAdd(key, _ => BuildMappingFunc(sourceType, destType));

        return (TDestination)compiled(source);
    }

    // ✅ NEW: Centralized mapping logic
    private Func<object, object> BuildMappingFunc(Type sourceType, Type destType)
    {
        var mapping = _mappings.FirstOrDefault(m =>
            m.CanHandle(sourceType, destType));

        if (mapping == null)
            throw new InvalidOperationException(
                $"Mapping not found: {sourceType.Name} → {destType.Name}");

        var key = (sourceType, destType);

        _typeMaps.TryGetValue(key, out var typeMap);

        return src =>
        {
            // ✅ fallback result from existing mapping
            var result = mapping.Map(src, new MappingContext());

            if (_ignoreNullValues)
            {
                ApplyIgnoreNullValues(src, result, sourceType, destType, typeMap ?? new TypeMap());
            }

            // ✅ apply advanced rules (ONLY if TypeMap exists)
            if (typeMap != null)
            {
                ApplyPropertyMaps(src, result, typeMap);
            }

            return result;
        };
    }

    // ✅ NEW: Apply ForMember / Ignore / Condition
    private void ApplyPropertyMaps(object source, object destination, TypeMap typeMap)
    {
        var destType = destination.GetType();

        foreach (var prop in destType.GetProperties())
        {
            if (!prop.CanWrite)
                continue;

            if (!typeMap.PropertyMaps.TryGetValue(prop.Name, out var map))
                continue;

            // ✅ Ignore
            if (map.Ignore)
            {
                SetDefaultPropertyValue(prop, destination);
                continue;
            }

            // ✅ Condition
            if (map.Condition != null && !map.Condition(source))
            {
                SetDefaultPropertyValue(prop, destination);
                continue;
            }

            // ✅ Custom Mapping
            if (map.CustomMapping != null)
            {
                var value = map.CustomMapping(source);
                prop.SetValue(destination, value);
            }
        }
    }

    private void ApplyIgnoreNullValues(object source, object destination, Type sourceType, Type destType, TypeMap typeMap)
    {
        var defaultInstance = Activator.CreateInstance(destType)!;

        foreach (var destProp in destType.GetProperties())
        {
            if (!destProp.CanWrite)
                continue;

            var sourceProp = sourceType.GetProperty(destProp.Name);
            if (sourceProp == null)
                continue;

            if (!sourceProp.PropertyType.IsClass && Nullable.GetUnderlyingType(sourceProp.PropertyType) == null)
                continue;

            var sourceValue = sourceProp.GetValue(source);
            if (sourceValue != null)
                continue;

            if (typeMap?.PropertyMaps.TryGetValue(destProp.Name, out var propertyMap) == true && propertyMap.CustomMapping != null)
                continue;

            var currentValue = destProp.GetValue(destination);
            if (currentValue != null)
                continue;

            var defaultValue = destProp.GetValue(defaultInstance);
            destProp.SetValue(destination, defaultValue);
        }
    }

    private void SetDefaultPropertyValue(System.Reflection.PropertyInfo prop, object destination)
    {
        var defaultValue = prop.PropertyType.IsValueType
            ? Activator.CreateInstance(prop.PropertyType)
            : null;

        prop.SetValue(destination, defaultValue);
    }

    // ✅ Detect collection types
    private bool IsCollection(System.Type sourceType, System.Type destType)
    {
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(sourceType)
            && typeof(System.Collections.IEnumerable).IsAssignableFrom(destType)
            && sourceType != typeof(string)
            && destType != typeof(string);
    }

    // ✅ Handle collection mapping
    private object MapCollection(object source, System.Type sourceType, System.Type destType)
    {
        var sourceEnumerable = (System.Collections.IEnumerable)source;

        var sourceItemType = GetCollectionItemType(sourceType);
        var destItemType = GetCollectionItemType(destType);

        if (sourceItemType == null || destItemType == null)
            throw new InvalidOperationException("Only generic collections are supported");

        var listType = typeof(List<>).MakeGenericType(destItemType);
        var destinationList = (System.Collections.IList)Activator.CreateInstance(listType)!;

        foreach (var item in sourceEnumerable)
        {
            var mappedItem = MapInternal(item!, sourceItemType, destItemType);
            destinationList.Add(mappedItem);
        }

        if (destType.IsArray)
        {
            var array = System.Array.CreateInstance(destItemType, destinationList.Count);
            destinationList.CopyTo(array, 0);
            return array;
        }

        if (destType.IsAssignableFrom(listType) || destType.IsInterface)
            return destinationList;

        if (Activator.CreateInstance(destType) is System.Collections.IList customList)
        {
            foreach (var item in destinationList)
                customList.Add(item);
            return customList;
        }

        throw new InvalidOperationException($"Destination collection type '{destType.Name}' is not supported");
    }

    private static System.Type GetCollectionItemType(System.Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
            return type.GetGenericArguments().FirstOrDefault();

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>));

        return enumerableInterface?.GetGenericArguments().FirstOrDefault();
    }

    // ✅ Internal mapping for nested/collection items
    private object MapInternal(object source, System.Type sourceType, System.Type destType)
    {
        var compiled = BuildMappingFunc(sourceType, destType);

        return compiled(source);
    }
}