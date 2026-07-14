namespace Transitio.Mediator;

/// <summary>
/// Marker for a notification (an event) published through <see cref="IPublisher.Publish{TNotification}"/>.
/// Zero or more <see cref="INotificationHandler{TNotification}"/> instances handle it.
/// </summary>
public interface INotification
{
}
