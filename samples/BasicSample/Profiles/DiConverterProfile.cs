using Transitio.Mapper;

public class DiConverterProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<OrderInput, OrderDomain_DI>()
                   .ConvertUsing<DiPricingConverter>();
    }
}