using System.Collections.Generic;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

public class AdvancedMappingTests
{
    [Fact]
    public void Should_Reuse_CreateMap_When_Called_Twice()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        var result = mapper.Map<UserDto>(new User { Name = "Hitesh", Age = 30 });

        Assert.Equal("Hitesh", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Should_Map_Collection_To_Array()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        var users = new List<User>
        {
            new User { Name = "A", Age = 20 },
            new User { Name = "B", Age = 25 }
        };

        var result = mapper.Map<UserDto[]>(users);

        Assert.IsType<UserDto[]>(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public void Should_Map_Collection_To_Interface_Type()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var mapper = config.BuildMapper();

        var users = new List<User>
        {
            new User { Name = "A", Age = 20 },
            new User { Name = "B", Age = 25 }
        };

        var result = mapper.Map<IList<UserDto>>(users);

        Assert.IsAssignableFrom<IList<UserDto>>(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public void Should_Map_Circular_Object_Graph_Without_StackOverflow()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Node, NodeDto>();
        });

        var root = new Node { Name = "Root" };
        var child = new Node { Name = "Child" };
        root.Next = child;
        child.Next = root;

        var mapper = config.BuildMapper();
        var result = mapper.Map<NodeDto>(root);

        Assert.Equal("Root", result.Name);
        Assert.NotNull(result.Next);
        Assert.Equal("Child", result.Next.Name);
        Assert.Same(result, result.Next.Next);
    }
}
