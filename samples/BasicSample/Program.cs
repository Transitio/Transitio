using BasicSample.MapperFeatures;
using BasicSample.DependencyFeatures;

// Core Transitio.Mapper features: profiles, ForMember, Condition, Ignore,
// reverse mapping, IgnoreNullValues, ConvertUsing, Include/IncludeBase, and
// nested collections.
MapperFeaturesDemo.Run();

Console.WriteLine();

// Transitio.Dependency (DI-layer) features: assembly scanning, direct IMapper
// injection, keyed mappers, scoped converters, and fail-fast validation.
DependencyFeaturesDemo.Run();
