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
 
               // ctx.Items: per-operation scratch data, shared with anything else mapped within
               // the SAME top-level Map() call (see Envelope/EnvelopeDomain below) - but not
               // visible across a fresh ctx.Mapper.Map(...) call, which gets its own context
               // (see ShipmentRequest/ShipmentDomain below).
               ctx.Items["RegionOverride"] = region;
 
               return new OrderDomain_Delegate
               {
                   FinalAmount = src.Amount,
                   Region = region
               };
           });
 
        // ctx.Items demo: Envelope.Order is auto-mapped first - nested-property mapping
        // (ApplyNestedMappings) always runs before ForMember's property maps (ApplyPropertyMaps)
        // for the same type - so by the time ResolvedRegion's MapFrom callback runs, the
        // OrderInput -> OrderDomain_Delegate converter above has already stashed the resolved
        // region into the context shared across this whole Map() call.
        cfg.CreateMap<Envelope, EnvelopeDomain>()
           .ForMember(dest => dest.ResolvedRegion, opt => opt.MapFrom((_, ctx) =>
               ctx.Items.TryGetValue("RegionOverride", out var region) ? region! : "none"));
 
        // ctx.Mapper demo: ShipmentRequest/ShipmentDomain has its own ConvertUsing, so automatic
        // nested-property mapping never runs for it - ctx.Mapper lets the converter opt back into
        // the full configured pipeline for the nested Order property instead of hand-rolling it.
        // Note this recursive Map<T>() call gets a brand-new MappingContext, so it does NOT see
        // ctx.Items set by an outer caller.
        cfg.CreateMap<ShipmentRequest, ShipmentDomain>()
           .ConvertUsing((src, ctx) => new ShipmentDomain
           {
               Order = ctx.Mapper!.Map<OrderDomain_Delegate>(src.Order),
               Carrier = src.Carrier
           });
    }
}