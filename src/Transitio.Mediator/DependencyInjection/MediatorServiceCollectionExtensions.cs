using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Transitio.Mediator;

public static class MediatorServiceCollectionExtensions
{
    private static readonly Type[] OpenHandlerInterfaces =
    {
        typeof(IRequestHandler<,>),
        typeof(INotificationHandler<>),
        typeof(IPipelineBehavior<,>),
    };

    /// <summary>
    /// Registers the mediator (<see cref="IMediator"/>, <see cref="ISender"/>, <see cref="IPublisher"/>)
    /// and scans the given assemblies for request handlers, notification handlers, and pipeline
    /// behaviors, registering each with a transient lifetime.
    /// </summary>
    public static IServiceCollection AddTransitioMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
        => services.AddTransitioMediator(ServiceLifetime.Transient, assemblies);

    /// <summary>
    /// Registers the mediator and scans the given assemblies for concrete
    /// <see cref="IRequestHandler{TRequest,TResponse}"/>, <see cref="INotificationHandler{TNotification}"/>,
    /// and closed <see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations, registering each
    /// discovered closed interface with the supplied <paramref name="lifetime"/>. Handlers are created
    /// by the container, so they may take constructor dependencies.
    /// </summary>
    /// <remarks>
    /// Open-generic pipeline behaviors (e.g. <c>LoggingBehavior&lt;,&gt;</c>) are not discovered by this
    /// closed-interface scan; register them manually, for example
    /// <c>services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;))</c>.
    /// </remarks>
    public static IServiceCollection AddTransitioMediator(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        // Register the mediator once and forward the three role interfaces to it. TryAdd keeps this
        // idempotent, so calling AddTransitioMediator more than once does not stack registrations.
        services.TryAdd(new ServiceDescriptor(typeof(Mediator), typeof(Mediator), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IMediator), sp => sp.GetRequiredService<Mediator>(), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(ISender), sp => sp.GetRequiredService<Mediator>(), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IPublisher), sp => sp.GetRequiredService<Mediator>(), lifetime));

        var handlerTypes = assemblies
            .Where(a => a != null)
            .SelectMany(GetLoadableTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (var type in handlerTypes)
        {
            var closedInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && OpenHandlerInterfaces.Contains(i.GetGenericTypeDefinition()))
                .ToList();

            if (closedInterfaces.Count == 0)
                continue;

            // Register each closed handler/behavior interface against the concrete type.
            // TryAddEnumerable dedupes by (serviceType, implementationType), so distinct handlers for
            // the same notification all register while a repeated scan of the same assembly does not
            // register (and later double-fire) the same handler twice.
            foreach (var iface in closedInterfaces)
                services.TryAddEnumerable(new ServiceDescriptor(iface, type, lifetime));
        }

        return services;
    }

    // Guards against ReflectionTypeLoadException when an assembly references types that cannot be
    // loaded; the types that did load are still usable. Mirrors the mapper's profile scan.
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
