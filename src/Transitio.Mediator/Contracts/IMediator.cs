namespace Transitio.Mediator;

/// <summary>
/// Entry point for sending requests and publishing notifications. Combines <see cref="ISender"/> and
/// <see cref="IPublisher"/>; resolve it from the container after calling <c>AddTransitioMediator</c>.
/// <code>
/// public record Ping(string Message) : IRequest&lt;string&gt;;
///
/// public class PingHandler : IRequestHandler&lt;Ping, string&gt;
/// {
///     public Task&lt;string&gt; Handle(Ping request, CancellationToken ct)
///         =&gt; Task.FromResult($"Pong: {request.Message}");
/// }
///
/// string reply = await mediator.Send(new Ping("hi"));
/// </code>
/// </summary>
public interface IMediator : ISender, IPublisher
{
}
