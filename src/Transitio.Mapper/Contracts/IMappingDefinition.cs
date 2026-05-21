using System;

namespace Transitio.Mapper;

public interface IMappingDefinition
{
    bool CanHandle(Type sourceType, Type destinationType);    
    object Map(object source, MappingContext context);
}