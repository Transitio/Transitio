#nullable enable
using System.Collections.Generic;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Verifies that a nested type's OWN mapping configuration (ForMember, ConvertUsing)
/// is honoured when that type appears as an auto-mapped nested member or collection element
/// - i.e. nested mapping runs the full mapper pipeline, not just plain property copying.
/// </summary>
public class NestedPipelineTests
{
    public class Child { public string Name { get; set; } = string.Empty; }
    public class ChildDto
    {
        public string Name { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }
    public class Parent
    {
        public Child Child { get; set; } = new();
        public List<Child> Children { get; set; } = new();
    }
    public class ParentDto
    {
        public ChildDto Child { get; set; } = new();
        public List<ChildDto> Children { get; set; } = new();
    }

    public class ChildConverter : ITypeConverter<Child, ChildDto>
    {
        public ChildDto Convert(Child source, IMappingContext context)
        => new ChildDto { Name = source.Name.ToUpper(), Tag = "converter" };
    }

    [Fact]
    public void Nested_Object_Applies_Childs_Own_ForMember()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Child, ChildDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.ToUpper()))
            .ForMember(d => d.Tag, opt => opt.MapFrom(s => "tagged"));
            cfg.CreateMap<Parent, ParentDto>();
        });
        var mapper = config.BuildMapper();
        var dto = mapper.Map<ParentDto>(new Parent { Child = new Child { Name = "alice" } });

        Assert.Equal("tagged", dto.Child.Tag);
    }

    [Fact]
    public void Nested_Collection_Applies_Childs_Own_Converter()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Child, ChildDto>().ConvertUsing<ChildConverter>();
            cfg.CreateMap<Parent, ParentDto>();
        });
        var mapper = config.BuildMapper();
        var dto = mapper.Map<ParentDto>(new Parent { Children = new List<Child> { new() { Name = "bob" }, new() { Name = "cara" } } });

        Assert.Equal(2, dto.Children.Count);
        Assert.Equal("BOB", dto.Children[0].Name);
        Assert.Equal("converted", dto.Children[0].Tag);
        Assert.Equal("CARA", dto.Children[1].Name);
        Assert.Equal("converted", dto.Children[1].Tag);
    }
}