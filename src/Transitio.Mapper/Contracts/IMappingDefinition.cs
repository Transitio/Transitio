using System;
 
namespace Transitio.Mapper;
 
public interface IMappingDefinition
{
    bool CanHandle(Type sourceType, Type destinationType);
    object Map(object source, MappingContext context);
 
    /// <summary>
    /// Maps <paramref name="source"/> onto an existing <paramref name="destination"/> instance in
    /// place, instead of allocating a new one. The default implementation (kept so any
    /// <see cref="IMappingDefinition"/> written before this member was added still compiles)
    /// delegates to <see cref="Map"/> and reflectively copies the result's public,
    /// readable/writable properties onto <paramref name="destination"/>.
    /// <see cref="MappingExpression{TSource,TDestination}"/> overrides this with a compiled,
    /// allocation-free assignment.
    /// </summary>
    void MapInto(object source, object destination, MappingContext context)
    {
        var mapped = Map(source, context);
        if (mapped == null)
            return;
 
        foreach (var prop in destination.GetType().GetProperties())
        {
            if (prop.CanRead && prop.CanWrite)
                prop.SetValue(destination, prop.GetValue(mapped));
        }
    }
}