#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Transitio.Mapper;

public class MappingExpression<TSource, TDestination> : IMappingDefinition
{
    private Func<TSource, TDestination>? _compiledFunc;
    private Func<TSource, TDestination>? _customFunc;

    private readonly TypeMap _typeMap;
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps;

    private readonly List<IMappingDefinition> _allMappings;

    // Thread-safe: the mapper is registered as a singleton and may be used concurrently;
    // this cache is populated during  Map() (via GetMaping) at runtime.
    private readonly ConcurrentDictionary<(Type, Type), IMappingDefinition?> _mappingCache = new();

    public MappingExpression(
     TypeMap typeMap,
     Dictionary<(Type, Type), TypeMap> typeMaps,
     List<IMappingDefinition> mappings)
    {
        _typeMap = typeMap;
        _typeMaps = typeMaps;
        _allMappings = mappings;
    }

    // ✅ Custom mapping
    public MappingExpression<TSource, TDestination> Using(Func<TSource, TDestination> func)
    {
        _customFunc = func;
        return this;
    }

    /// <summary>
    /// Specifies a type converter to handle the entire type transformation.
    /// The converter type will be instantiated when mapping occurs.
    /// </summary>
    /// <typeparam name="TConverter">The converter type implementing ITypeConverter&lt;TSource, TDestination&gt;</typeparam>
    public MappingExpression<TSource, TDestination> ConvertUsing<TConverter>()
        where TConverter : ITypeConverter<TSource, TDestination>
    {
        _typeMap.ConverterType = typeof(TConverter);
        _typeMap.ConverterInstance = null;
        _typeMap.ConverterDelegate = null;
        return this;
    }

    /// <summary>
    /// Specifies a pre-instantiated type converter to handle the entire type transformation.
    /// </summary>
    /// <param name="converter">The converter instance to use</param>
    public MappingExpression<TSource, TDestination> ConvertUsing(ITypeConverter<TSource, TDestination> converter)
    {
        _typeMap.ConverterInstance = converter;
        _typeMap.ConverterType = null;
        _typeMap.ConverterDelegate = null;
        return this;
    }

    /// <summary>
    /// Specifies a custom delegate converter to handle the entire type transformation.
    /// The delegate receives the source and mapping context for full control.
    /// </summary>
    /// <param name="converter">The converter delegate</param>
    public MappingExpression<TSource, TDestination> ConvertUsing(Func<TSource, IMappingContext, TDestination> converter)
    {
        _typeMap.ConverterDelegate = converter;
        _typeMap.ConverterInstance = null;
        _typeMap.ConverterType = null;
        return this;
    }

    /// <summary>
    /// Includes mappings from a base/parent type, applying them before derived type mappings.
    /// The derived type mappings can override base mappings through ForMember configurations.
    /// </summary>
    /// <typeparam name="TBaseSource">The base source type</typeparam>
    /// <typeparam name="TBaseDestination">The base destination type</typeparam>
    public MappingExpression<TSource, TDestination> Include<TBaseSource, TBaseDestination>()
    {
        // Validate that base types are actually base types of source/destination
        if (!typeof(TBaseSource).IsAssignableFrom(typeof(TSource)))
        {
            throw new InvalidOperationException(
                $"Cannot include mapping {typeof(TBaseSource).Name} -> {typeof(TBaseDestination).Name}: " +
                $"{typeof(TBaseSource).Name} is not a base type of {typeof(TSource).Name}");
        }

        var includedKey = (typeof(TBaseSource), typeof(TBaseDestination));

        // Check for circular includes
        if (WouldCreateCircularInclude(includedKey, new HashSet<(Type, Type)>()))
        {
            throw new InvalidOperationException(
                $"Circular include detected: {typeof(TSource).Name} -> {typeof(TDestination).Name} " +
                $"includes {typeof(TBaseSource).Name} -> {typeof(TBaseDestination).Name}");
        }

        _typeMap.IncludedMaps.Add(includedKey);
        return this;
    }

    /// <summary>
    /// Includes mappings from a base type. Alias for Include for explicit base class mapping.
    /// </summary>
    /// <typeparam name="TBaseSource">The base source type</typeparam>
    /// <typeparam name="TBaseDestination">The base destination type</typeparam>
    public MappingExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
    {
        return Include<TBaseSource, TBaseDestination>();
    }

    private bool WouldCreateCircularInclude(
        (Type, Type) includedKey,
        HashSet<(Type, Type)> visited)
    {
        if (visited.Contains(includedKey))
            return true;

        visited.Add(includedKey);

        if (_typeMaps.TryGetValue(includedKey, out var includedTypeMap))
        {
            foreach (var nestedInclude in includedTypeMap.IncludedMaps)
            {
                if (WouldCreateCircularInclude(nestedInclude, new HashSet<(Type, Type)>(visited)))
                    return true;
            }
        }

        return false;
    }

    // ✅ Single correct implementation
    public bool CanHandle(System.Type sourceType, System.Type destinationType)
    {
        return sourceType == typeof(TSource) &&
               destinationType == typeof(TDestination);
    }

    public object Map(object source, MappingContext context)
    {
        if (context.ObjectCache.TryGetValue(source, out var cached))
            return cached;

        var customFunc = _customFunc;
        if (customFunc != null)
        {
            var customResult = customFunc((TSource)source);
            context.ObjectCache[source] = customResult!;
            return customResult!;
        }

        var compiled = _compiledFunc ??= BuildExpression();
        var result = compiled((TSource)source);
        context.ObjectCache[source] = result!;

        // ✅ Nested mapping
        foreach (var destProp in typeof(TDestination).GetProperties())
        {
            if (!destProp.CanWrite)
                continue;

            var sourceProp = typeof(TSource).GetProperty(destProp.Name);

            if (sourceProp == null)
                continue;

            if (IsSimpleType(destProp.PropertyType))
                continue;

            var sourceValue = sourceProp.GetValue(source);

            if (sourceValue == null)
                continue;

            // Auto-map nested collection properties (e.g. List<Order> -> List<OrderDto>).
            if (IsCollectionType(sourceProp.PropertyType) && IsCollectionType(destProp.PropertyType))
            {
                var mappedCollection = MapNestedCollection(sourceValue, sourceProp.PropertyType, destProp.PropertyType, context);
                if (mappedCollection != null)
                {
                    destProp.SetValue(result, mappedCollection);
                }
                continue;
            }

            var mapping = GetMapping(sourceProp.PropertyType, destProp.PropertyType);

            if (mapping != null)
            {
                var mappedValue = MapThroughPipeline(sourceValue, sourceProp.PropertyType, destProp.PropertyType, mapping, context);
                destProp.SetValue(result, mappedValue);
            }
        }

        return result!;
    }

    public MappingExpression<TSource, TDestination> ForMember<TMember>(
    Expression<Func<TDestination, TMember>> dest,
    Action<MemberOptions<TSource>> config)
    {
        // ✅ Extract property name
        var member = dest.Body as MemberExpression;

        if (member == null && dest.Body is UnaryExpression unary)
        {
            member = unary.Operand as MemberExpression;
        }

        if (member == null)
            throw new InvalidOperationException("Invalid expression");

        var propName = member.Member.Name;

        // ✅ Get or create PropertyMap
        if (!_typeMap.PropertyMaps.TryGetValue(propName, out var propertyMap))
        {
            propertyMap = new PropertyMap
            {
                DestinationProperty = propName
            };

            _typeMap.PropertyMaps[propName] = propertyMap;
        }

        // ✅ Apply configuration
        var options = new MemberOptions<TSource>(propertyMap);

        config(options);

        return this;
    }

    public MappingExpression<TDestination, TSource> ReverseMap()
    {
        var reverseKey = (_typeMap.DestinationType, _typeMap.SourceType);

        // ✅ Check if mapping already exists
        if (_typeMaps.TryGetValue(reverseKey, out var existing))
        {
            var existingExpression = new MappingExpression<TDestination, TSource>(existing, _typeMaps, _allMappings);

            if (!_allMappings.Any(m => m.CanHandle(_typeMap.DestinationType, _typeMap.SourceType)))
            {
                _allMappings.Add(existingExpression);
            }

            return existingExpression;
        }

        // ✅ Create reverse TypeMap
        var reverseTypeMap = new TypeMap
        {
            SourceType = _typeMap.DestinationType,
            DestinationType = _typeMap.SourceType
        };

        // ✅ OPTIONAL: Copy simple property mappings
        foreach (var kvp in _typeMap.PropertyMaps)
        {
            var original = kvp.Value;

            // ⚠️ Only reverse simple mappings (not custom functions)
            if (!original.Ignore && original.CustomMapping == null)
            {
                reverseTypeMap.PropertyMaps[kvp.Key] = new PropertyMap
                {
                    DestinationProperty = kvp.Key
                };
            }
        }

        // ✅ Register reverse mapping
        _typeMaps[reverseKey] = reverseTypeMap;

        var reverseExpression = new MappingExpression<TDestination, TSource>(reverseTypeMap, _typeMaps, _allMappings);
        _allMappings.Add(reverseExpression);

        return reverseExpression;
    }

    // ✅ Expression builder
    private Func<TSource, TDestination> BuildExpression()
    {
        var sourceParam = Expression.Parameter(typeof(TSource), "src");

        var bindings = new List<MemberBinding>();

        foreach (var destProp in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destProp.CanWrite)
                continue;

            var sourceProp = typeof(TSource).GetProperty(destProp.Name);

            if (sourceProp == null)
                continue;

            if (!IsSimpleType(destProp.PropertyType))
                continue;

            var sourceExpr = Expression.Property(sourceParam, sourceProp);

            bindings.Add(Expression.Bind(destProp, sourceExpr));
        }

        var body = Expression.MemberInit(
            Expression.New(typeof(TDestination)),
            bindings
        );

        var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

        return lambda.Compile();
    }

    // ✅ Helper
    private bool IsSimpleType(System.Type type)
    {
        // Treat Nullable<T> the same type as T(e.g. int?,DateTime?, Guid?) so nullable
        // value-type members are copied directly instead of being silently skipped.
        var t = Nullable.GetUnderlyingType(type) ?? type;

        return t.IsPrimitive
            || t.IsEnum
            || t == typeof(string)
            || t == typeof(DateTime)
            || t == typeof(decimal)
            || t == typeof(Guid);
    }

    private IMappingDefinition? GetMapping(Type sourceType, Type destType)
    {
        var key = (sourceType, destType);

        if (_mappingCache.TryGetValue(key, out var mapping))
            return mapping;

        if (!_typeMaps.ContainsKey(key))
        {
            _mappingCache[key] = null;
            return null;
        }

        mapping = _allMappings.FirstOrDefault(m => m.CanHandle(sourceType, destType));
        _mappingCache[key] = mapping;
        return mapping;
    }

    // Route nested mapping through the mapper's full pipeline so the nested type's own
    // ConvertUsing / ForMember / Ignore / Condition / ignore-null settings are applied.
    // Fall back to the raw definition if the context has no TransitioMapper attached.
    private static object MapThroughPipeline(
        object source,
        Type sourceType,
        Type destType,
        IMappingDefinition mapping,
        MappingContext context)
    {
        return context.Mapper is TransitioMapper mapper
        ? mapper.MapWithContext(source, sourceType, destType, context)
        : mapping.Map(source, context);
    }

    // Maps a nested collection property, reusing element type maps and the shared
    // mapping context (so cycle detection still applies). Returns null when no element
    // mapping is configured, leaving the destination property at its default.
    private object? MapNestedCollection(object sourceValue, Type sourceType, Type destType, MappingContext context)
    {
        var srcItemType = GetCollectionItemType(sourceType);
        var destItemType = GetCollectionItemType(destType);

        if (srcItemType == null || destItemType == null)
            return null;

        var itemMapping = GetMapping(srcItemType, destItemType);

        if (itemMapping == null)
            return null;

        var listType = typeof(List<>).MakeGenericType(destItemType);
        var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

        foreach (var item in (System.Collections.IEnumerable)sourceValue)
        {
            list.Add(item == null ? null : MapThroughPipeline(item, srcItemType, destItemType, itemMapping, context));
        }

        if (destType.IsArray)
        {
            var array = Array.CreateInstance(destItemType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }

        if (destType.IsAssignableFrom(listType) || destType.IsInterface)
        {
            return list;
        }
        if (Activator.CreateInstance(destType) is System.Collections.IList customList)
        {
            foreach (var x in list)
            {
                customList.Add(x);
            }
            return customList;
        }
        return list;
    }

    private static bool IsCollectionType(Type type)
    {
        return type != typeof(string)
        && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private static Type? GetCollectionItemType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
            return type.GetGenericArguments().FirstOrDefault();

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments().FirstOrDefault();
    }
}