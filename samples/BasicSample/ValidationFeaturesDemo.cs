using Microsoft.Extensions.DependencyInjection;
using Transitio.Validation;
 
namespace BasicSample.ValidationFeatures;
 
// ============================================================================
// Models and validator used by the Transitio.Validation feature demo.
// ============================================================================
 
public class Customer
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}
 
// Declare rules in the constructor with RuleFor(...). Every rule runs on each
// Validate call (Continue cascade), so all failures are reported at once.
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50);
 
        RuleFor(c => c.Email)
            .NotNull()
            .EmailAddress();
 
        RuleFor(c => c.Age)
            .InclusiveBetween(0, 150);
    }
}
 
// A second property demonstrates CascadeMode.StopOnFirstFailure: the chain stops at
// the first failing validator instead of collecting every failure for that property.
public class StrictCustomerValidator : AbstractValidator<Customer>
{
    public StrictCustomerValidator()
    {
        RuleFor(c => c.Name)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Name is required")
            .Matches("^[A-Z].*").WithMessage("Name must start with a capital letter");
    }
}
 
// A second model/validator covering the built-in validators Customer/StrictCustomer don't
// exercise: Must, Equal/NotEqual, Length, MinimumLength, GreaterThan(OrEqual)/LessThan(OrEqual),
// and WithErrorCode.
public class OrderRequest
{
    public string? Sku { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
 
public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(o => o.Sku).Length(3, 10).WithErrorCode("SKU_LENGTH");
        RuleFor(o => o.Notes).MinimumLength(5);
 
        RuleFor(o => o.Status).Equal("Pending");
        RuleFor(o => o.Status).NotEqual("Cancelled");
 
        RuleFor(o => o.Quantity).GreaterThan(0).LessThan(1000);
        RuleFor(o => o.Quantity).Must(q => q % 2 == 0).WithMessage("Quantity must be even (sold in pairs)");
 
        // Instance-aware Must((T, TProperty) => bool) - the predicate sees the whole object,
        // not just the Quantity value, enabling a cross-field check.
        RuleFor(o => o.Quantity)
            .Must((order, qty) => qty * order.Price <= 100000m)
            .WithMessage("Quantity * Price must not exceed 100000");
 
        RuleFor(o => o.Price).GreaterThanOrEqual(0.01m).LessThanOrEqual(100000m);
    }
}
 
public static class ValidationFeaturesDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Transitio.Validation features ===");
 
        var validator = new CustomerValidator();
 
        // 1. A valid instance passes.
        var valid = new Customer { Name = "Ada Lovelace", Email = "ada@example.com", Age = 36 };
        Console.WriteLine($"Valid customer -> IsValid: {validator.Validate(valid).IsValid}");
 
        // 2. An invalid instance reports every failure at once.
        var invalid = new Customer { Name = "", Email = "not-an-email", Age = 999 };
        var result = validator.Validate(invalid);
        Console.WriteLine($"Invalid customer -> IsValid: {result.IsValid}, errors: {result.Errors.Count}");
        foreach (var failure in result.Errors)
            Console.WriteLine($"  - [{failure.ErrorCode}] {failure.ErrorMessage}");
 
        // 3. ValidateAndThrow throws a ValidationException carrying the result.
        try
        {
            validator.ValidateAndThrow(invalid);
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"ValidateAndThrow threw with {ex.Errors.Count} error(s).");
        }
 
        // 4. CascadeMode.StopOnFirstFailure: only the first failing validator on the
        //    property is reported, instead of every one of them.
        var strictValidator = new StrictCustomerValidator();
        var strictResult = strictValidator.Validate(new Customer { Name = "" });
        Console.WriteLine($"Cascade.StopOnFirstFailure -> errors: {strictResult.Errors.Count} ({strictResult.Errors[0].ErrorMessage})");
 
        // 5. DI: scan an assembly and resolve IValidator<T>.
        var provider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();
 
        var resolved = provider.GetRequiredService<IValidator<Customer>>();
        Console.WriteLine($"Resolved validator from DI -> valid passes: {resolved.Validate(valid).IsValid}");
 
        // 6. Built-in validators not covered above: Must, Equal/NotEqual, Length, MinimumLength,
        //    GreaterThan(OrEqual)/LessThan(OrEqual), and WithErrorCode.
        var orderValidator = new OrderRequestValidator();
 
        var validOrder = new OrderRequest { Sku = "SKU-123", Notes = "standard order", Status = "Pending", Quantity = 4, Price = 19.99m };
        Console.WriteLine($"Valid order -> IsValid: {orderValidator.Validate(validOrder).IsValid}");
 
        var invalidOrder = new OrderRequest { Sku = "AB", Notes = "hi", Status = "Cancelled", Quantity = 3, Price = 0m };
        var orderResult = orderValidator.Validate(invalidOrder);
        Console.WriteLine($"Invalid order -> IsValid: {orderResult.IsValid}, errors: {orderResult.Errors.Count}");
        foreach (var failure in orderResult.Errors)
            Console.WriteLine($"  - [{failure.ErrorCode}] {failure.PropertyName}: {failure.ErrorMessage}");
 
        // 7. ServiceLifetime override: the default AddTransitioValidation(assembly) overload
        //    registers validators as Singleton. Passing ServiceLifetime.Transient instead resolves
        //    a brand-new instance every time, provable by comparing object references across two
        //    resolutions from the same provider.
        var singletonProvider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();
        var singletonA = singletonProvider.GetRequiredService<IValidator<Customer>>();
        var singletonB = singletonProvider.GetRequiredService<IValidator<Customer>>();
        Console.WriteLine($"Default (Singleton) lifetime -> same instance: {ReferenceEquals(singletonA, singletonB)}");
 
        var transientProvider = new ServiceCollection()
            .AddTransitioValidation(ServiceLifetime.Transient, typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();
        var transientA = transientProvider.GetRequiredService<IValidator<Customer>>();
        var transientB = transientProvider.GetRequiredService<IValidator<Customer>>();
        Console.WriteLine($"ServiceLifetime.Transient override -> same instance: {ReferenceEquals(transientA, transientB)}");
 
        // 8. Equal/NotEqual's optional IEqualityComparer<T>: without a comparer, default equality
        //    is case-sensitive, so "Pending" fails Equal("PENDING"). With StringComparer
        //    .OrdinalIgnoreCase, the same values compare equal.
        var caseSensitive = new InlineValidator<OrderRequest>(v =>
            v.Rule(o => o.Status).Equal("PENDING"));
        var caseInsensitive = new InlineValidator<OrderRequest>(v =>
            v.Rule(o => o.Status).Equal("PENDING", StringComparer.OrdinalIgnoreCase));
 
        var pendingOrder = new OrderRequest { Status = "Pending" };
        Console.WriteLine($"Equal(\"PENDING\") default comparer -> IsValid: {caseSensitive.Validate(pendingOrder).IsValid}");
        Console.WriteLine($"Equal(\"PENDING\", OrdinalIgnoreCase) -> IsValid: {caseInsensitive.Validate(pendingOrder).IsValid}");
 
        // 9. Value-based validators treat null as a skip, so they compose cleanly with NotNull()
        //    instead of producing duplicate failures for the same null value.
        var skipsNull = new InlineValidator<OrderRequest>(v =>
            v.Rule(o => o.Notes).MinimumLength(5));
        var composedWithNotNull = new InlineValidator<OrderRequest>(v =>
            v.Rule(o => o.Notes).NotNull().MinimumLength(5));
 
        var nullNotesOrder = new OrderRequest { Notes = null };
        Console.WriteLine($"MinimumLength(5) alone on null Notes -> IsValid (null is skipped): {skipsNull.Validate(nullNotesOrder).IsValid}");
        var composedResult = composedWithNotNull.Validate(nullNotesOrder);
        Console.WriteLine($"NotNull().MinimumLength(5) on null Notes -> IsValid: {composedResult.IsValid}, errors: {composedResult.Errors.Count}");
    }
}
 
// A minimal AbstractValidator<T> wrapper for one-off rules that don't warrant a dedicated named
// validator class, used by the Equal-comparer and null-skip demos above. RuleFor itself is
// protected (validators call it from their own constructor), so Rule(...) is a public forwarder.
public sealed class InlineValidator<T> : AbstractValidator<T>
{
    public InlineValidator(Action<InlineValidator<T>> configure) => configure(this);
    public IRuleBuilder<T, TProperty> Rule<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression)
        => RuleFor(expression);
}