using System;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mapper;

namespace Transitio.Dependency;

public class TransitioDependency
{
    private readonly IServiceProvider _sp;

    public TransitioDependency(IServiceProvider sp)
    {
        _sp = sp;
        Mapping = new MappingDependency(sp);
    }

    public MappingDependency Mapping { get; }
}