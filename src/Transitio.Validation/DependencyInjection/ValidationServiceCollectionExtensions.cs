using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Transitio.Validation;

public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Scans the given assemblies for concrete <see cref="IValidator{T}"/> implementations and
    /// registers each as its closed interface (and as its concrete type) with a singleton
    /// lifetime. Validators are stateless, so singleton is the safe default.
    /// </summary>
    public static IServiceCollection AddTransitioValidation(
        this IServiceCollection services,
        params Assembly[] assemblies)
        => services.AddTransitioValidation(ServiceLifetime.Singleton, assemblies);

    /// <summary>
    /// Scans the given assemblies for concrete <see cref="IValidator{T}"/> implementations with
    /// a public parameterless constructor and registers each with the supplied
    /// <paramref name="lifetime"/>. The concrete type is registered once and every closed
    /// <see cref="IValidator{T}"/> it implements forwards to it, so a single instance is shared
    /// per scope. Validators that need constructor dependencies should be registered manually.
    /// </summary>
    public static IServiceCollection AddTransitioValidation(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        var openValidator = typeof(IValidator<>);

        var validatorTypes = assemblies
            .Where(a => a != null)
            .SelectMany(GetLoadableTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
                && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var type in validatorTypes)
        {
            var closedInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openValidator)
                .ToList();

            if (closedInterfaces.Count == 0)
                continue;

            // Register the concrete type once, then forward each closed IValidator<T> to it so
            // they all resolve to the same instance within a scope.
            services.Add(new ServiceDescriptor(type, type, lifetime));

            foreach (var iface in closedInterfaces)
                services.Add(new ServiceDescriptor(iface, sp => sp.GetRequiredService(type), lifetime));
        }

        return services;
    }

    // Guards against ReflectionTypeLoadException when an assembly references types that cannot
    // be loaded; the types that did load are still usable. Mirrors the mapper's profile scan.
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
