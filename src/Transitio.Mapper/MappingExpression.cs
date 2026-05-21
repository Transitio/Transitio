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

    private readonly List<IMappingDefinition> _allMappings;

    public MappingExpression(List<IMappingDefinition> mappings)
    {
        _allMappings = mappings;
    }

    // ✅ Custom mapping
    public MappingExpression<TSource, TDestination> Using(Func<TSource, TDestination> func)
    {
        _customFunc = func;
        return this;
    }

    // ✅ Single correct implementation
    public bool CanHandle(System.Type sourceType, System.Type destinationType)
    {
        return sourceType == typeof(TSource) &&
               destinationType == typeof(TDestination);
    }

    public object Map(object source, MappingContext context)
    {
        if (_customFunc != null)
            return _customFunc((TSource)source);

        _compiledFunc ??= BuildExpression();

        var result = _compiledFunc((TSource)source);

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

            var mapping = _allMappings.FirstOrDefault(m =>
                m.CanHandle(sourceProp.PropertyType, destProp.PropertyType));

            if (mapping != null)
            {
                var mappedValue = mapping.Map(sourceValue, context);
                destProp.SetValue(result, mappedValue);
            }
        }

        return result!;
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
}