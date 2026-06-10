using Transitio.Mapper;
//Type-based converter with a constructor dependency, resolved from the DI container.
//By Transitio.Depenedncy when registered via ConvertUsing<DiPricingConverter>().
public class DiPricingConverter 
    : ITypeConverter<OrderInput, OrderDomain_DI>
{
    private readonly PricingService _service;

    public DiPricingConverter(PricingService service)
    {
        _service = service;
    }    

    OrderDomain_DI ITypeConverter<OrderInput, OrderDomain_DI>.Convert(OrderInput source, IMappingContext context)
    => new()
    {
        FinalAmount = _service.ApplyDiscount(source.Amount),
        Region = "DI-Discounted"
    };
}