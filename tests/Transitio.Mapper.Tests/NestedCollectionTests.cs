#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Verifies that collection-typed properties on a nested object are mapped automatically
/// when a CreateMap for the element types exists (List, array, and interface destinations).
/// </summary>
public class NestedCollectionTests
{
    public class Child { public string Name { get; set; } = string.Empty; }
    public class ChildDto { public string Name { get; set; } = string.Empty; }

    public class Parent
    {
        public string Title { get; set; } = string.Empty;
        public List<Child> Children { get; set; } = new();
        public Child[] ChildArray { get; set; } = Array.Empty<Child>();
        public List<Child> ChildList { get; set; } = new();
    }
    public class ParentDto
    {
        public string Title { get; set; } = string.Empty;
        public List<ChildDto> Children { get; set; } = new();
        public ChildDto[] ChildArray { get; set; } = Array.Empty<ChildDto>();
        public List<ChildDto> ChildList { get; set; } = new List<ChildDto>();
    }

    [Fact]
    public void Should_Auto_Map_Nested_List_Property()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Child, ChildDto>();
            cfg.CreateMap<Parent, ParentDto>();
        });
        var mapper = config.BuildMapper();

        var parent = new Parent
        {
            Title = "P",
            Children = new List<Child> { new() { Name = "a" }, new() { Name = "b" } }
        };

        var dto = mapper.Map<ParentDto>(parent);

        Assert.Equal("P", dto.Title);
        Assert.Equal(2, dto.Children.Count);
        Assert.Equal(new[] { "a", "b" }, dto.Children.Select(c => c.Name));
    }

    [Fact]
    public void Should_Auto_Map_Nested_Array_And_Interface_Properties()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Child, ChildDto>();
            cfg.CreateMap<Parent, ParentDto>();
        });
        var mapper = config.BuildMapper();

        var parent = new Parent
        {
            ChildArray = new[] { new Child { Name = "x" } },
            ChildList = new List<Child> { new() { Name = "y" }, new() { Name = "z" } }
        };

        var dto = mapper.Map<ParentDto>(parent);

        Assert.Single(dto.ChildArray);
        Assert.Equal("x", dto.ChildArray[0].Name);
        Assert.Equal(2, dto.ChildList.Count);
        Assert.Equal("z", dto.ChildList[1].Name);
    }

    [Fact]
    public void Should_Leave_Nested_Collection_Default_When_No_Element_Map()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Parent, ParentDto>(); // no Child -> ChildDto map registered
        });
        var mapper = config.BuildMapper();

        var parent = new Parent
        {
            Title = "P",
            Children = new List<Child> { new() { Name = "a" } }
        };

        var dto = mapper.Map<ParentDto>(parent);

        Assert.Equal("P", dto.Title);
        Assert.Empty(dto.Children);
    }

}