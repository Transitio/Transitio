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
    /// Open-generic pipeline behaviors (e.g. <c>LoggingBehavior&lt;TRequest,TResponse&gt;</c>, generic in
    /// both type parameters) implementing <see cref="IPipelineBehavior{TRequest,TResponse}"/> are
    /// discovered and registered against the open <c>IPipelineBehavior&lt;,&gt;</c> service type, so the
    /// container builds a closed behavior for whatever request/response pair is resolved. Closed
    /// behaviors from the same scan are always registered first, so they wrap open-generic behaviors
    /// from the outside — this ordering falls out of the two-pass scan and is not independently
    /// configurable. A <b>partially</b> closed open-generic behavior (e.g. generic only in
    /// <c>TRequest</c>, with a fixed response type) is <b>not</b> discovered — only fully open
    /// implementations, generic in both type parameters, are auto-registered; register a partial one
    /// manually. Manual registration, e.g.
    /// <c>services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;))</c>,
    /// also remains available when a specific behavior needs a different lifetime. Open-generic
    /// <see cref="IRequestHandler{TRequest,TResponse}"/>/<see cref="INotificationHandler{TNotification}"/>
    /// implementations are not discovered by this scan.
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
 
        var allTypes = assemblies
            .Where(a => a != null)
            .SelectMany(GetLoadableTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();
 
        foreach (var type in allTypes.Where(t => !t.IsGenericTypeDefinition))
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
 
        // Open-generic pipeline behaviors, e.g. LoggingBehavior<TRequest,TResponse>. Registered
        // against the OPEN IPipelineBehavior<,> service type so the container constructs a closed
        // IPipelineBehavior<TRequest,TResponse> for whatever request/response pair is resolved.
        // RequestHandlerWrapper<TRequest,TResponse> already resolves via
        // GetServices<IPipelineBehavior<TRequest,TResponse>>(), and .NET's DI container natively
        // mixes closed and open-generic registrations for the same service, so no change is needed
        // there. Scoped to IPipelineBehavior<,> only: open-generic IRequestHandler<,>/
        // INotificationHandler<> would be ambiguous to auto-register (multiple concrete request
        // types could match), so those still require explicit closed registration.
        foreach (var type in allTypes.Where(t => t.IsGenericTypeDefinition))
        {
            var pipelineInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
 
            if (pipelineInterface == null)
                continue;
 
            // Require the interface's TRequest/TResponse to be exactly the type's own generic
            // parameters, in the same order — i.e. genuinely `IPipelineBehavior<TRequest, TResponse>`
            // over the type's own two parameters, not (e.g.) IPipelineBehavior<TRequest, string> with
            // a padded, unused second type parameter, or IPipelineBehavior<TRequest, TRequest> reusing
            // one parameter twice. A plain arity/count check alone would miss both of those shapes,
            // registering an implementation that can't actually satisfy every closed request MakeGenericType
            // constructs it for, and crashing at first resolution instead of being skipped safely here.
            if (!pipelineInterface.GetGenericArguments().SequenceEqual(type.GetGenericArguments()))
                continue;
 
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, lifetime));
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