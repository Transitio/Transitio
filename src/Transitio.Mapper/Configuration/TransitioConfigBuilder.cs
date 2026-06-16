using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Transitio.Mapper;

public class TransitioConfigBuilder
{
    public bool IgnoreNullValues { get; private set; }

    public TransitioConfigBuilder SetIgnoreNullValues(bool value)
    {
        IgnoreNullValues = value;
        return this;
    }

    /// <summary>
    /// When set, the configuration is validated as soon as it is built (fail fast at
    /// startup) instead of at the first <c>Map</c> call. Validation is applied after all
    /// inline maps and any assembly-scanned profiles have been registered.
    /// </summary>
    public bool ShouldValidateConfiguration { get; private set; }

    /// <summary>
    /// Opts in to eager configuration validation. Throws an
    /// <see cref="System.InvalidOperationException"/> at build time if any mapping is
    /// invalid (missing or type-mismatched properties without a converter / nested map).
    /// </summary>
    public TransitioConfigBuilder ValidateConfiguration()
    {
        ShouldValidateConfiguration = true;
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

    /// <summary>
    /// Scans the given assemblies for concrete <see cref="MappingProfile"/> types with a
    /// parameterless constructor, instantiates each one, and applies its configuration.
    /// </summary>
    public TransitioConfigBuilder AddProfilesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        var profileType = typeof(MappingProfile);

        var profileTypes = assemblies
            .Where(a => a != null)
            .SelectMany(GetLoadableTypes)
            .Where(t => profileType.IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface
                        && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var type in profileTypes)
        {
            var profile = (MappingProfile)Activator.CreateInstance(type)!;
            profile.Configure(this);
        }

        return this;
    }

    /// <summary>
    /// Scans the assembly containing <typeparamref name="TMarker"/> for mapping profiles.
    /// </summary>
    public TransitioConfigBuilder AddProfilesFromAssemblyContaining<TMarker>()
        => AddProfilesFromAssemblies(typeof(TMarker).Assembly);

    // Guards against ReflectionTypeLoadException when an assembly references types that
    // cannot be loaded; the types that did load are still usable.
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}