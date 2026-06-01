using System.Collections.Generic;

namespace Transitio.Mapper;

public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public new bool Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(object obj)
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
