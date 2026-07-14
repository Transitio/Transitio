using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Transitio.Mediator;

// Bridges the type-erased Send(object) call to a strongly-typed handler + pipeline. One closed
// wrapper is created per request type and cached on the mediator, so the reflection to build it
// happens once per request type rather than per Send.
internal abstract class RequestHandlerWrapperBase<TResponse>
{
    public abstract Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapperBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typed = (TRequest)request;

        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null)
            throw new InvalidOperationException(
                $"No handler registered for request '{typeof(TRequest)}'. Register an " +
                $"IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> via AddTransitioMediator.");

        RequestHandlerDelegate<TResponse> handlerCall = () => handler.Handle(typed, cancellationToken);

        // Behaviors run in registration order around the handler: fold the list in reverse so the
        // first-registered behavior ends up outermost.
        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        var pipeline = behaviors.Aggregate(
            handlerCall,
            (next, behavior) => () => behavior.Handle(typed, next, cancellationToken));

        return pipeline();
    }
}
