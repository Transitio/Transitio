using Transitio.Mapper;

public class DelegateConverterProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<OrderInput, OrderDomain_Delegate>()
           .ConvertUsing((src, ctx) =>
           {
               if (src == null)
                   throw new ArgumentNullException(nameof(src));

               var region = src.Country == "IN" ? "APAC" : "GLOBAL";

               // Optional: context override (advanced use case)
               if (ctx?.Items?.TryGetValue("RegionOverride", out var value) == true)
               {
                   region = value?.ToString() ?? region;
               }

               return new OrderDomain_Delegate
               {
                   FinalAmount = src.Amount,
                   Region = region
               };
           });
    }
}