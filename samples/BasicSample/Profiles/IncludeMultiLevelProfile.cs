using Transitio.Mapper;

public class IncludeMultiLevelProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        // Level 1
        cfg.CreateMap<Person, PersonDto>();

        // Level 2
        cfg.CreateMap<Employee, EmployeeDto>()
           .IncludeBase<Person, PersonDto>();

        // Level 3
        cfg.CreateMap<Manager, ManagerDto>()
           .IncludeBase<Employee, EmployeeDto>();
    }
}