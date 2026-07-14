using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Default <see cref="IMediator"/> implementation. Resolves handlers and behaviors from the
/// container per call and caches the per-type dispatch wrappers, so it is thread-safe and can be
/// registered with any lifetime.
/// </summary>
internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Type, object> _requestWrappers = new();
    private readonly ConcurrentDictionary<Type, NotificationHandlerWrapperBase> _notificationWrappers = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var wrapper = (RequestHandlerWrapperBase<TResponse>)_requestWrappers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
                return Activator.CreateInstance(wrapperType)!;
            });

        return wrapper.Handle(request, _serviceProvider, cancellationToken);
    }

    /// <inheritdoc />
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
        => Send<Unit>(request, cancellationToken);

    /// <inheritdoc />
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        return PublishCore(notification, cancellationToken);
    }

    /// <inheritdoc />
    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));
        if (notification is not INotification)
            throw new ArgumentException(
                $"'{notification.GetType()}' does not implement INotification.", nameof(notification));

        return PublishCore(notification, cancellationToken);
    }

    private Task PublishCore(object notification, CancellationToken cancellationToken)
    {
        // Key on the runtime type so a notification published via a base reference still reaches the
        // handlers registered for its concrete type.
        var wrapper = _notificationWrappers.GetOrAdd(
            notification.GetType(),
            notificationType =>
            {
                var wrapperType = typeof(NotificationHandlerWrapper<>).MakeGenericType(notificationType);
                return (NotificationHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;
            });

        return wrapper.Handle(notification, _serviceProvider, cancellationToken);
    }
}
