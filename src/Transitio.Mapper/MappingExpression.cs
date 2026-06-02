#nullable enable
using System;
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
    private readonly Dictionary<(Type, Type), IMappingDefinition?> _mappingCache = new();

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

            var mapping = GetMapping(sourceProp.PropertyType, destProp.PropertyType);

            if (mapping != null)
            {
                var mappedValue = mapping.Map(sourceValue, context);
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
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(DateTime)
            || type == typeof(decimal)
            || type == typeof(Guid);
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
}