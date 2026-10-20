using Transitio.Mapper;

// Uses ContactPerson/Supervisor - NOT Person/Employee - because CreateMap dedups by type pair and
// returns the SAME shared TypeMap for repeated calls. IncludeBasicProfile and
// IncludeMultiLevelProfile both also map Person->PersonDto / Employee->EmployeeDto; if this
// profile overrode Name on that same pair, the override would leak into those other two demos'
// output too (confirmed: it used to print "EMP: Hitesh" in "Include: Basic", which is supposed to
// show the override-free case). A dedicated type hierarchy keeps this demo's override isolated.
public class IncludeOverrideProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
    {
        // Base mapping
        cfg.CreateMap<ContactPerson, ContactPersonDto>()
           .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name));

        // Derived mapping overrides base
        cfg.CreateMap<Supervisor, SupervisorDto>()
           .IncludeBase<ContactPerson, ContactPersonDto>()
           .ForMember(d => d.Name, opt => opt.MapFrom(s => $"EMP: {s.Name}"));
    }
}