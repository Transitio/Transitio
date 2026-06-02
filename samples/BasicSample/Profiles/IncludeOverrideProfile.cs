using Transitio.Mapper;

public class IncludeOverrideProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        // Base mapping
        cfg.CreateMap<Person, PersonDto>()
           .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name));

        // Derived mapping overrides base
        cfg.CreateMap<Employee, EmployeeDto>()
           .IncludeBase<Person, PersonDto>()
           .ForMember(d => d.Name, opt => opt.MapFrom(s => $"EMP: {s.Name}"));
    }
}