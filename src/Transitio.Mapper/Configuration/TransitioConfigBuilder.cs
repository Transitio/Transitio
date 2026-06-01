using System;
using System.Collections.Generic;
using System.Linq;

namespace Transitio.Mapper;

public class TransitioConfigBuilder
{
    public bool IgnoreNullValues { get; private set; }

    public TransitioConfigBuilder SetIgnoreNullValues(bool value)
    {
        IgnoreNullValues = value;
        return this;
    }
    private readonly List<IMappingDefinition> _mappings;

    // ✅ NEW: TypeMaps (for ForMember, Ignore, etc.)
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps;

    public Dictionary<(Type, Type), TypeMap> GetTypeMaps()
    {
        return _typeMaps;
    }

    public TransitioConfigBuilder(List<IMappingDefinition> mappings, Dictionary<(Type, Type), TypeMap> typeMaps)
    {
        _mappings = mappings;
        _typeMaps = typeMaps;
    }

    public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);
        var key = (sourceType, destType);

        if (_typeMaps.TryGetValue(key, out var existingTypeMap))
        {
            var existingExpression = _mappings
                .OfType<MappingExpression<TSource, TDestination>>()
                .FirstOrDefault(m => m.CanHandle(sourceType, destType));

            if (existingExpression != null)
                return existingExpression;

            // If a type map exists but no matching expression is present, continue and create one.
        }

        // ✅ Create TypeMap
        var typeMap = new TypeMap
        {
            SourceType = sourceType,
            DestinationType = destType
        };

        _typeMaps[key] = typeMap;

        // ✅ Use NEW constructor (and KEEP mappings)
        var expression = new MappingExpression<TSource, TDestination>(
            typeMap,
            _typeMaps,
            _mappings
        );

        _mappings.Add(expression);

        return expression;
    }


    public void AddProfile<TProfile>()
        where TProfile : MappingProfile, new()
    {
        var profile = new TProfile();
        profile.Configure(this);
    }

    public void AddProfiles(params MappingProfile[] profiles)
    {
        foreach (var profile in profiles)
        {
            profile.Configure(this);
        }
    }

}