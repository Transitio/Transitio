using Transitio.Mapper;

namespace Transitio.Mapper.Tests;

public class OrderProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<Order, OrderDto>();
        cfg.CreateMap<User, UserDto>();
    }
}
