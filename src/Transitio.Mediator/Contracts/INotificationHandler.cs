using System.Threading;
using System.Threading.Tasks;

namespace Transitio.Mediator;

/// <summary>
/// Handles a published <typeparamref name="TNotification"/>. Any number of handlers may exist for a
/// notification; all are invoked when it is published.
/// </summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>Handles the notification.</summary>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
