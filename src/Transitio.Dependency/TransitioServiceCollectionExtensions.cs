using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mapper;

namespace Transitio.Dependency;

public static class TransitioServiceCollectionExtensions
{
    public static IServiceCollection AddTransitio(
        this IServiceCollection services,
        Action<TransitioConfigBuilder> config)
        => services.AddTransitio(ServiceLifetime.Singleton, config, Array.Empty<Assembly>());

    /// <summary>
    /// Registers Transitio and discovers all mapping profiles in the given assemblies.
    /// </summary>
    public static IServiceCollection AddTransitio(
        this IServiceCollection services,
        params Assembly[] assemblies)
        => services.AddTransitio(ServiceLifetime.Singleton, _ => { }, assemblies);

    /// <summary>
    /// Registers Transitio, applies the inline configuration, and then discovers all
    /// mapping profiles in the given assemblies.
    /// </summary>
    public static IServiceCollection AddTransitio(
        this IServiceCollection services,
        Action<TransitioConfigBuilder> config,
        params Assembly[] assemblies)
        => services.AddTransitio(ServiceLifetime.Singleton, config, assemblies);

    /// <summary>
    /// Registers Transitio with an explicit service lifetime, applies the inline
    /// configuration, and discovers all mapping profiles in the given assemblies.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ServiceLifetime.Scoped"/> (or <see cref="ServiceLifetime.Transient"/>)
    /// when a <c>ConvertUsing&lt;TConverter&gt;</c> converter has constructor dependencies
    /// that are themselves scoped (for example a <c>DbContext</c>). The converter is then
    /// instantiated from the same scope as the resolved mapper, avoiding a captive
    /// dependency on the root provider. The default <see cref="ServiceLifetime.Singleton"/>
    /// keeps the compiled-mapping cache shared across the whole application; non-singleton
    /// lifetimes rebuild that cache per scope.
    /// </remarks>
    public static IServiceCollection AddTransitio(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        Action<TransitioConfigBuilder> config,
        params Assembly[] assemblies)
    {
        var mapperConfig = BuildConfiguration(config, assemblies);

        // Build the mapper lazily so type-based converters can be resolved from the
        // container (allowing converters with constructor dependencies). The captured
        // provider matches the registration lifetime, so a scoped/transient mapper
        // resolves its converters from that same scope.
        services.Add(new ServiceDescriptor(
            typeof(IMapper),
            sp => mapperConfig.BuildMapper(type => ActivatorUtilities.CreateInstance(sp, type)),
            lifetime));

        services.Add(new ServiceDescriptor(
            typeof(TransitioDependency),
            sp => new TransitioDependency(sp),
            lifetime));

        return services;
    }

    /// <summary>
    /// Registers a keyed Transitio mapper and discovers all mapping profiles in the given
    /// assemblies.
    /// </summary>
    public static IServiceCollection AddKeyedTransitio(
        this IServiceCollection services,
        object serviceKey,
        params Assembly[] assemblies)
        => services.AddKeyedTransitio(serviceKey, ServiceLifetime.Singleton, _ => { }, assemblies);

    /// <summary>
    /// Registers a keyed Transitio mapper, applies the inline configuration, and discovers
    /// all mapping profiles in the given assemblies.
    /// </summary>
    public static IServiceCollection AddKeyedTransitio(
        this IServiceCollection services,
        object serviceKey,
        Action<TransitioConfigBuilder> config,
        params Assembly[] assemblies)
        => services.AddKeyedTransitio(serviceKey, ServiceLifetime.Singleton, config, assemblies);

    /// <summary>
    /// Registers a keyed Transitio mapper with an explicit service lifetime. Multiple
    /// independent mapper configurations can be registered side by side under different
    /// keys, then resolved with
    /// <c>GetRequiredKeyedService&lt;IMapper&gt;(serviceKey)</c> or the
    /// <c>[FromKeyedServices(serviceKey)]</c> attribute.
    /// </summary>
    public static IServiceCollection AddKeyedTransitio(
        this IServiceCollection services,
        object serviceKey,
        ServiceLifetime lifetime,
        Action<TransitioConfigBuilder> config,
        params Assembly[] assemblies)
    {
        if (serviceKey == null)
            throw new ArgumentNullException(nameof(serviceKey));

        var mapperConfig = BuildConfiguration(config, assemblies);

        services.Add(new ServiceDescriptor(
            typeof(IMapper),
            serviceKey,
            (sp, _) => mapperConfig.BuildMapper(type => ActivatorUtilities.CreateInstance(sp, type)),
            lifetime));

        return services;
    }

    // Shared: builds the mapper configuration from inline config plus any assemblies to
    // scan for profiles. Validation (if opted in via cfg.ValidateConfiguration()) runs
    // here, after every map and scanned profile has been registered.
    private static TransitioMapperConfiguration BuildConfiguration(
        Action<TransitioConfigBuilder> config,
        Assembly[] assemblies)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        return new TransitioMapperConfiguration(builder =>
        {
            config(builder);

            if (assemblies.Length > 0)
                builder.AddProfilesFromAssemblies(assemblies);
        });
    }
}