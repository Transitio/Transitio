#nullable enable
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

    //optional factory for instantiating type-based converters
    //when supplied (e.g. by Transitio.Dependency), converters can have constructor
    // dependencies resolved from the DI container. otherwise we fall back to Activator.
    private readonly Func<Type, object>? _converterFactory;

    //CACHE (Key =(source, destination)); compiled pipeline takes the shared context.
    private readonly ConcurrentDictionary<(Type, Type), Func<object, MappingContext, object>> _cache = new();

    public TransitioMapper(
        List<IMappingDefinition> mappings,
        Dictionary<(Type, Type), TypeMap> typeMaps,
        bool ignoreNullValues,
        Func<Type, object>? converterFactory = null)
    {
        _mappings = mappings;
        _typeMaps = typeMaps;
        _ignoreNullValues = ignoreNullValues;
        _converterFactory = converterFactory;
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // One  context per top-level call; shared across the whole object graph so
        // nested mapping reuses cycle detection and the full mapping pipeline.
        var context = new MappingContext { Mapper = this };

        return (TDestination)MapCore(source, source.GetType(), typeof(TDestination), context);
    }

    // Full mapping pipeline (converter /property maps / ignore-null) for a single
    // object, resuing an existing context. Used for both top level and nested mapping
    // so that a nested type's own ConvertUsing / ForMember / Ignore / Condition apply.
    internal object MapWithContext(object source, Type sourceType, Type destType, MappingContext context)
    {
        return MapCore(source, sourceType, destType, context);
    }

    private object MapCore(object source, Type sourceType, Type destType, MappingContext context)
    {
        // Cycle short-circuit: an object already mapped in this graph returns its result.
        if (context.ObjectCache.TryGetValue(source, out var cached))
            return cached;

        // ✅ Collection support
        if (IsCollection(sourceType, destType))
            return MapCollection(source, sourceType, destType, context);

        var compiled = _cache.GetOrAdd((sourceType, destType), _ => BuildMappingFunc(sourceType, destType));

        return compiled(source, context);
    }

    // ✅ NEW: Centralized mapping logic
    private Func<object, MappingContext, object> BuildMappingFunc(Type sourceType, Type destType)
    {
        var mapping = _mappings.FirstOrDefault(m =>
            m.CanHandle(sourceType, destType));

        if (mapping == null)
            throw new InvalidOperationException(
                $"Mapping not found: {sourceType.Name} → {destType.Name}");

        var key = (sourceType, destType);

        _typeMaps.TryGetValue(key, out var typeMap);

        return (src, context) =>
        {
            // ✅ Check if converter is configured - execute it instead of property mapping
            if (typeMap != null && TryExecuteConverter(src, sourceType, destType, typeMap, context, out var convertedResult))
            {
                context.ObjectCache[src] = convertedResult!;
                return convertedResult!;
            }

            // ✅ fallback result from existing mapping
            var result = mapping.Map(src, context);

            if (_ignoreNullValues)
            {
                ApplyIgnoreNullValues(src, result, sourceType, destType, typeMap ?? new TypeMap());
            }

            // ✅ apply advanced rules (ONLY if TypeMap exists)
            if (typeMap != null)
            {
                ApplyPropertyMaps(src, result, typeMap, context);
            }

            return result;
        };
    }

    // ✅ NEW: Try to execute a configured type converter
    private bool TryExecuteConverter(object source, Type sourceType, Type destType, TypeMap typeMap, MappingContext context, out object? result)
    {
        result = null;

        // ✅ Instance-based converter
        if (typeMap.ConverterInstance != null)
        {
            result = InvokeConverter(typeMap.ConverterInstance, sourceType, destType, source, context);
            return true;
        }

        // ✅ Type-based converter (auto-instantiate, optionally via DI factory)
        if (typeMap.ConverterType != null)
        {
            var converterInstance = _converterFactory != null
                ? _converterFactory(typeMap.ConverterType)
                : Activator.CreateInstance(typeMap.ConverterType);

            if (converterInstance == null)
            {
                throw new InvalidOperationException(
                    $"Could not create an instance of Converter '{typeMap.ConverterType.Name}' ." +
                    "If using a DI factory, ensure the converter type is registered."
                );
            }
            result = InvokeConverter(converterInstance, sourceType, destType, source, context);
            return true;
        }

        // ✅ Delegate-based converter
        if (typeMap.ConverterDelegate != null)
        {
            result = InvokeDelegateConverter(typeMap.ConverterDelegate, source, context);
            return true;
        }

        return false;
    }

    // Invoke a converter through its ITypeConverter<,> interface so that BOTH
    // implicit and explicit interface implementations dispatch correctly.
    private static object? InvokeConverter(object converter, Type sourceType, Type destType, object source, MappingContext context)
    {
        var convertMethod = ResolveConvertMethod(converter.GetType(), sourceType, destType);
        if (convertMethod == null)
            throw new InvalidOperationException($"Converter '{converter.GetType().Name}' does not implement ITypeConverter<{sourceType.Name}, {destType.Name}>");

        try
        {
            return convertMethod.Invoke(converter, new[] { source, context });
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap the inner exception for better error clarity
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // Unreachable
        }
    }

    // Invoke a delegate based converter, surfacing the real exception rather than
    // swallowing it and silently falling back to property mapping. 
    private static object? InvokeDelegateConverter(Delegate converterDelegate, object source, MappingContext context)
    {
        try
        {
            return converterDelegate.DynamicInvoke(source, context);
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap the inner exception for better error clarity
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // Unreachable
        }
    }

    //Find the ITypeConverter<,>. Convert method matching the (source, dest) pair.
    private static System.Reflection.MethodInfo? ResolveConvertMethod(Type converterType, Type sourceType, Type destType)
    {
        System.Reflection.MethodInfo? fallback = null;

        foreach (var ifc in converterType.GetInterfaces())
        {
            if (ifc.IsGenericType || ifc.GetGenericTypeDefinition() != typeof(ITypeConverter<,>))
                continue;
            var args = ifc.GetGenericArguments();
            var method = ifc.GetMethod("Convert");

            //
            if (args[0] == sourceType && args[1] == destType)
            {
                return method;
            }
            if (args[0].IsAssignableFrom(sourceType) && destType.IsAssignableFrom(args[1]))
            {
                fallback ??= method;
            }
        }

        return fallback;
    }


    // ✅ NEW: Apply ForMember / Ignore / Condition (with Include support)
    private void ApplyPropertyMaps(object source, object destination, TypeMap typeMap, MappingContext context)
    {
        // ✅ First apply included type mappings (base type mappings)
        foreach (var (baseSourceType, baseDestType) in typeMap.IncludedMaps)
        {
            if (_typeMaps.TryGetValue((baseSourceType, baseDestType), out var baseTypeMap))
            {
                ApplyPropertyMaps(source, destination, baseTypeMap, context);
            }
        }

        // ✅ Then apply derived type mappings (can override base mappings)
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

            // ✅ Custom Mapping (with context support)
            if (map.CustomMappingWithContext != null)
            {
                var value = map.CustomMappingWithContext(source, context);
                prop.SetValue(destination, value);
            }
            // ✅ Custom Mapping (simple, backward compatible)
            else if (map.CustomMapping != null)
            {
                var value = map.CustomMapping(source);
                prop.SetValue(destination, value);
            }
        }
    }

    private static readonly ConcurrentDictionary<Type, object> _defaultInstances = new();

    private void ApplyIgnoreNullValues(object source, object destination, Type sourceType, Type destType, TypeMap typeMap)
    {
        var defaultInstance = _defaultInstances.GetOrAdd(destType, t => Activator.CreateInstance(t)!);

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

            if (typeMap?.PropertyMaps.TryGetValue(destProp.Name, out var propertyMap) == true &&
            (propertyMap.CustomMapping != null || propertyMap.CustomMappingWithContext != null))
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
    private object MapCollection(object source, System.Type sourceType, System.Type destType, MappingContext context)
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
            // Each element runs the full pipeline (converters/ property maps) and share the context.
            var mappedItem = item == null ? null : MapWithContext(item, sourceItemType, destItemType, context);
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

    private static System.Type? GetCollectionItemType(System.Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
            return type.GetGenericArguments().FirstOrDefault();

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>));

        return enumerableInterface?.GetGenericArguments().FirstOrDefault();
    }
}