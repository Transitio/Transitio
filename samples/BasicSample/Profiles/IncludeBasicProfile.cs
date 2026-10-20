using Transitio.Mapper;

public class IncludeBasicProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        // Base mapping
        cfg.CreateMap<Person, PersonDto>();

        // Derived mapping reuses base. Include<>() and IncludeBase<>() are the same feature
        // (IncludeBase is an alias) - this profile uses the Include<>() spelling.
        cfg.CreateMap<Employee, EmployeeDto>()
           .Include<Person, PersonDto>();
    }
}