using Transitio.Mediator;
using Transitio.Mapper;
using Transitio.Validation;
 
namespace Transitio.Integration.Tests;
 
// ============================================================================
// A realistic "place an order" flow shared by the integration tests in this
// project: a Mediator request is validated (Transitio.Validation) by an
// open-generic pipeline behavior before its handler maps the request to a
// result (Transitio.Mapper). Exercises all four runtime packages composed in
// a single DI container, the way a real application would.
// ============================================================================
 
public sealed class PlaceOrderRequest : IRequest<OrderResult>
{
    public string CustomerName { get; set; } = "";
    public string? Email { get; set; }
    public decimal Amount { get; set; }
}
 
public sealed class OrderResult
{
    public string CustomerName { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
}
 
public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderValidator()
    {
        RuleFor(r => r.CustomerName).NotEmpty();
        RuleFor(r => r.Email).NotNull().EmailAddress();
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
 
// Records handler invocations without any shared static state — each test registers its own
// instance as a singleton in its own ServiceCollection, so tests never interfere with each other.
public sealed class OrderProcessingTracker
{
    public int ProcessedCount;
}
 
public sealed class PlaceOrderHandler : IRequestHandler<PlaceOrderRequest, OrderResult>
{
    private readonly IMapper _mapper;
    private readonly OrderProcessingTracker _tracker;
 
    public PlaceOrderHandler(IMapper mapper, OrderProcessingTracker tracker)
    {
        _mapper = mapper;
        _tracker = tracker;
    }
 
    public Task<OrderResult> Handle(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<OrderResult>(request);
        result.Status = "Placed";
        _tracker.ProcessedCount++;
        return Task.FromResult(result);
    }
}
 
// An open-generic pipeline behavior: discovered automatically by AddTransitioMediator's assembly
// scan (Milestone 1), and constructor-injected with whatever IValidator<TRequest>s are registered
// for the concrete request type — zero validators for a request type that doesn't need one is
// fine, since it's a collection, not a required single dependency.
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
 
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        foreach (var validator in _validators)
        {
            var result = validator.Validate(request);
            if (!result.IsValid)
                throw new ValidationException(result);
        }
 
        return await next();
    }
}