using System;
using System.Collections.Generic;
using System.Linq;

namespace Transitio.Mapper;

public class TransitioMapper : IMapper
{
    private readonly List<IMappingDefinition> _mappings;

    // ✅ CACHE (key = (source, destination))
    private readonly Dictionary<(System.Type, System.Type), Func<object, object>> _cache = new();

    public TransitioMapper(List<IMappingDefinition> mappings)
    {
        _mappings = mappings;
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

        // ✅ CACHE HIT
        if (_cache.TryGetValue(key, out var mapFunc))
        {
            return (TDestination)mapFunc(source);
        }

        // ✅ First-time build
        var mapping = _mappings.FirstOrDefault(m =>
            m.CanHandle(sourceType, destType));

        if (mapping == null)
            throw new InvalidOperationException(
                $"Mapping not found: {sourceType.Name} → {destType.Name}");

        Func<object, object> compiled = src =>
            mapping.Map(src, new MappingContext());

        // ✅ Store in cache
        _cache[key] = compiled;

        return (TDestination)compiled(source);
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

        var sourceItemType = sourceType.GetGenericArguments().FirstOrDefault();
        var destItemType = destType.GetGenericArguments().FirstOrDefault();

        if (sourceItemType == null || destItemType == null)
            throw new InvalidOperationException("Only generic collections are supported");

        var listType = typeof(List<>).MakeGenericType(destItemType);
        var destinationList = (System.Collections.IList)Activator.CreateInstance(listType)!;

        foreach (var item in sourceEnumerable)
        {
            var mappedItem = MapInternal(item!, sourceItemType, destItemType);
            destinationList.Add(mappedItem);
        }

        return destinationList;
    }

    // ✅ Internal mapping for nested/collection items
    private object MapInternal(object source, System.Type sourceType, System.Type destType)
    {
        var mapping = _mappings.FirstOrDefault(m =>
            m.CanHandle(sourceType, destType));

        if (mapping == null)
            throw new InvalidOperationException(
                $"Mapping not found: {sourceType.Name} → {destType.Name}");

        return mapping.Map(source, new MappingContext());
    }
}