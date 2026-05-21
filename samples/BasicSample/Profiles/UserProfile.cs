using Transitio.Mapper;

public class UserProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        cfg.CreateMap<User, UserDto>();
    }
}