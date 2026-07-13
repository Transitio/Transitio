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

        // 4. DI: scan an assembly and resolve IValidator<T>.
        var provider = new ServiceCollection()
            .AddTransitioValidation(typeof(CustomerValidator).Assembly)
            .BuildServiceProvider();

        var resolved = provider.GetRequiredService<IValidator<Customer>>();
        Console.WriteLine($"Resolved validator from DI -> valid passes: {resolved.Validate(valid).IsValid}");
    }
}
