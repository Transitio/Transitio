using Transitio.Mapper;

public class IncludeBasicProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        // Base mapping
        cfg.CreateMap<Person, PersonDto>();

        // Derived mapping reuses base
        cfg.CreateMap<Employee, EmployeeDto>()
           .IncludeBase<Person, PersonDto>();
    }
}