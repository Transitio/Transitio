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
        var mapper = mapperConfig.BuildMapper();

        services.AddSingleton<IMapper>(mapper);
        services.AddSingleton<TransitioDependency>();

        return services;
    }
}