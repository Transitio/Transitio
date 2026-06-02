using Transitio.Mapper;

public class TypeConverterProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<OrderInput, OrderDomain_Type>()
                   .ConvertUsing<OrderConverter>();
    }
}