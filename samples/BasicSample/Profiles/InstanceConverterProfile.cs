using Transitio.Mapper;

public class InstanceConverterProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        var pricingService = new PricingService();

        cfg.CreateMap<OrderInput, OrderDomain_Instance>()
           .ConvertUsing(new PricingConverter(pricingService));
    }
}