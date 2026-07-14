using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Publishes notifications to every registered <see cref="INotificationHandler{TNotification}"/>.
/// </summary>
public interface IPublisher
{
    /// <summary>Publishes a notification to all of its handlers.</summary>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publishes a notification whose static type is not known at the call site. The runtime type
    /// must implement <see cref="INotification"/>.
    /// </summary>
    Task Publish(object notification, CancellationToken cancellationToken = default);
}
