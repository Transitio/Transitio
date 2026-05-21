using System;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Mapper;

namespace Transitio.Dependency;

public class MappingDependency
{
    private readonly IServiceProvider _sp;

    public MappingDependency(IServiceProvider sp)
    {
        _sp = sp;
    }

    public IMapper Mapper => _sp.GetRequiredService<IMapper>();
}