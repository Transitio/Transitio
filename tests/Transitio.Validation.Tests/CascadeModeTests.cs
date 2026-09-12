using System.Linq;
 
namespace Transitio.Validation.Tests;
 
public class CascadeModeTests
{
    private static ValidationResult Run(Customer customer, System.Action<ConfigurableValidator> configure)
        => new ConfigurableValidator(configure).Validate(customer);
 
    [Fact]
    public void Continue_Is_Default_And_Reports_All_Failures()
    {
        var result = Run(new Customer { Name = "" }, v => v.For(c => c.Name)
            .NotEmpty()
            .MinimumLength(5));
 
        Assert.Equal(2, result.Errors.Count);
    }
 
    [Fact]
    public void StopOnFirstFailure_Stops_After_First_Failing_Validator()
    {
        var result = Run(new Customer { Name = "" }, v => v.For(c => c.Name)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty()
            .MinimumLength(5));
 
        var failure = Assert.Single(result.Errors);
        Assert.Equal("Name", failure.PropertyName);
    }
 
    [Fact]
    public void StopOnFirstFailure_Does_Not_Short_Circuit_When_First_Validator_Passes()
    {
        var result = Run(new Customer { Name = "ab" }, v => v.For(c => c.Name)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty()
            .MinimumLength(5));
 
        Assert.Single(result.Errors);
    }
 
    [Fact]
    public void Cascade_Only_Affects_The_Configured_Property()
    {
        var result = Run(new Customer { Name = "", Email = "not-an-email" }, v =>
        {
            v.For(c => c.Name)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .MinimumLength(5);
 
            v.For(c => c.Email)
                .Matches("^nonmatching$")
                .EmailAddress();
        });
 
        Assert.Equal(3, result.Errors.Count);
        Assert.Single(result.Errors, e => e.PropertyName == "Name");
        Assert.Equal(2, result.Errors.Count(e => e.PropertyName == "Email"));
    }
 
    [Fact]
    public void Cascade_Can_Be_Set_At_The_End_Of_The_Chain()
    {
        var result = Run(new Customer { Name = "" }, v => v.For(c => c.Name)
            .NotEmpty()
            .MinimumLength(5)
            .Cascade(CascadeMode.StopOnFirstFailure));
 
        Assert.Single(result.Errors);
    }
}