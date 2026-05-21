using System.Collections.Generic;

namespace Transitio.Mapper;

public class TransitioConfigBuilder
{
    private readonly List<IMappingDefinition> _mappings;

    public TransitioConfigBuilder(List<IMappingDefinition> mappings)
    {
        _mappings = mappings;
    }

    public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var expression = new MappingExpression<TSource, TDestination>(_mappings);
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