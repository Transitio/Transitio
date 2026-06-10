using System;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mapper;

namespace Transitio.Dependency;

public static class TransitioServiceCollectionExtensions
{
    public static IServiceCollection AddTransitio(
        this IServiceCollection services,
        Action<TransitioConfigBuilder> config)
    {
        var mapperConfig = new TransitioMapperConfiguration(config);        

        //Build the mapper lazily so type-based converters can be resolvedfrom the
        //container (allowing converters with constructor dependencies).
        services.AddSingleton<IMapper>(sp=>
                    mapperConfig.BuildMapper(type => ActivatorUtilities.CreateInstance(sp, type)));
        services.AddSingleton<TransitioDependency>();

        return services;
    }
}