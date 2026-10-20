// Models for the IMappingContext demo (ctx.Items / ctx.Mapper) in Profiles/DelegateConverterProfile.cs.
 
public class Envelope
{
    public OrderInput Order { get; set; } = new() { Currency = "INR", Country = "IN" };
}
 
public class EnvelopeDomain
{
    public OrderDomain_Delegate Order { get; set; } = new() { Region = "" };
    public string ResolvedRegion { get; set; } = "";
}
 
public class ShipmentRequest
{
    public OrderInput Order { get; set; } = new() { Currency = "INR", Country = "IN" };
    public string Carrier { get; set; } = "";
}
 
public class ShipmentDomain
{
    public OrderDomain_Delegate Order { get; set; } = new() { Region = "" };
    public string Carrier { get; set; } = "";
}