using Transitio.Mapper;

public class PricingService
{
    public decimal ApplyDiscount(decimal amount) => amount * 0.9m;
}

public class PricingConverter 
    : ITypeConverter<OrderInput, OrderDomain_Instance>
{
    private readonly PricingService _service;

    public PricingConverter(PricingService service)
    {
        _service = service;
    }

    OrderDomain_Instance ITypeConverter<OrderInput, OrderDomain_Instance>.Convert(OrderInput source, IMappingContext context)
    {
          return new OrderDomain_Instance
        {
            FinalAmount = _service.ApplyDiscount(source.Amount),
            Region = "Discounted"
        };
    }
}