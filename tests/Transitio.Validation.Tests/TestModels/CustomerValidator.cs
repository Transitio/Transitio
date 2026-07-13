namespace Transitio.Validation.Tests;

/// <summary>Sample validator used by the AbstractValidator and DI tests.</summary>
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Email).NotNull().EmailAddress();
        RuleFor(c => c.Age).InclusiveBetween(0, 150);
    }
}
