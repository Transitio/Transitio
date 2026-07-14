using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Transitio.Mediator;

// Mirror of RequestHandlerWrapper for notifications: one closed wrapper per notification type,
// cached on the mediator.
internal abstract class NotificationHandlerWrapperBase
{
    public abstract Task Handle(
        object notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapperBase
    where TNotification : INotification
{
    public override async Task Handle(
        object notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typed = (TNotification)notification;

        // Handlers run sequentially in registration order. Every handler is invoked even if an
        // earlier one throws, so one faulty handler does not hide the rest; collected failures are
        // surfaced together as an AggregateException. Zero handlers is a no-op.
        List<Exception>? failures = null;
        foreach (var handler in serviceProvider.GetServices<INotificationHandler<TNotification>>())
        {
            try
            {
                await handler.Handle(typed, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                (failures ??= new List<Exception>()).Add(ex);
            }
        }

        if (failures != null)
            throw new AggregateException(
                "One or more notification handlers threw an exception.", failures);
    }
}
