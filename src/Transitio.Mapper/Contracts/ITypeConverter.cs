#nullable enable

namespace Transitio.Mapper;

/// <summary>
/// Represents a custom type converter that handles entire type transformations with custom logic.
/// Converters provide a way to replace the default property-by-property mapping with custom conversion logic.
/// </summary>
/// <typeparam name="TSource">The source type to convert from</typeparam>
/// <typeparam name="TDestination">The destination type to convert to</typeparam>
public interface ITypeConverter<in TSource, out TDestination>
{
    /// <summary>
    /// Converts a source instance to a destination instance using custom logic.
    /// </summary>
    /// <param name="source">The source instance to convert</param>
    /// <param name="context">The mapping context providing access to the mapper for recursive conversions</param>
    /// <returns>The converted destination instance</returns>
    TDestination Convert(TSource source, IMappingContext context);
}
