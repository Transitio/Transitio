using System;
 
namespace Transitio.Mapper;
 
public interface IMapper
{
    TDestination Map<TDestination>(object source);
 
    /// <summary>
    /// Maps <paramref name="source"/> onto the given <paramref name="destination"/> instance, in
    /// place, and returns it. The default implementation (kept so any <see cref="IMapper"/>
    /// implementation written before this member was added still compiles) delegates to
    /// <see cref="Map{TDestination}(object)"/> and reflectively copies the result onto
    /// <paramref name="destination"/>; <see cref="TransitioMapper"/> overrides this with an
    /// optimized, allocation-free path.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
 
        var mapped = Map<TDestination>(source);
        foreach (var prop in typeof(TDestination).GetProperties())
        {
            if (prop.CanRead && prop.CanWrite)
                prop.SetValue(destination, prop.GetValue(mapped));
        }
        return destination;
    }
}