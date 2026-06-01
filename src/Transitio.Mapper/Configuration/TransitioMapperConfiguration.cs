using System;
using System.Collections.Generic;
using System.Linq;

namespace Transitio.Mapper;

public class TransitioMapperConfiguration
{
    private readonly List<IMappingDefinition> _mappings = new();
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps = new(); // ✅ NEW: TypeMaps   
    private readonly bool _ignoreNullValues;

    public TransitioMapperConfiguration(Action<TransitioConfigBuilder> config)
    {
        var builder = new TransitioConfigBuilder(_mappings, _typeMaps);
        config(builder);
        _ignoreNullValues = builder.IgnoreNullValues;
    }

    public IMapper BuildMapper()
    {
        return new TransitioMapper(_mappings, _typeMaps, _ignoreNullValues);
    }

    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var mapping in _mappings)
        {
            ValidateMapping(mapping, errors);
        }

        if (errors.Any())
        {
            throw new InvalidOperationException(
                "Mapping validation failed:\n" + string.Join("\n", errors)
            );
        }
    }
    private void ValidateMapping(IMappingDefinition mapping, List<string> errors)
    {
        var type = mapping.GetType();

        if (!type.IsGenericType)
            return;

        var genericArgs = type.GetGenericArguments();

        var sourceType = genericArgs[0];
        var destType = genericArgs[1];

        var sourceProps = sourceType.GetProperties();
        var destProps = destType.GetProperties();

        foreach (var destProp in destProps)
        {
            var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name);

            // ✅ Missing source property
            if (sourceProp == null)
            {
                errors.Add($"Missing source property: {sourceType.Name}.{destProp.Name} for {destType.Name}");
                continue;
            }

            // ✅ Type mismatch (basic check)
            if (!AreTypesCompatible(sourceProp.PropertyType, destProp.PropertyType))
            {
                errors.Add($"Type mismatch: {sourceType.Name}.{destProp.Name} → {destType.Name}.{destProp.Name}");
            }
        }
    }
    private bool AreTypesCompatible(Type sourceType, Type destType)
    {
        if (sourceType == destType)
            return true;

        if (destType.IsAssignableFrom(sourceType))
            return true;

        // ✅ Allow nested mapping if mapping exists
        return _mappings.Any(m =>
            m.CanHandle(sourceType, destType));
    }
}