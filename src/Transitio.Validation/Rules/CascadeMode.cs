namespace Transitio.Validation;
 
/// <summary>Controls whether a property's validators keep running after the first failure.</summary>
public enum CascadeMode
{
    /// <summary>Run every validator on the property, collecting all failures (the default).</summary>
    Continue,
 
    /// <summary>Stop at the first failed validator for this property.</summary>
    StopOnFirstFailure,
}