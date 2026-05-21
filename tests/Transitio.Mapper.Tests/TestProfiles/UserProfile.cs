using Transitio.Mapper;

namespace Transitio.Mapper.Tests;

public class UserProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<User, UserDto>();
    }
}