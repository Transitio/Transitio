using System;

public class MemberOptions<TSource>
{
    private readonly PropertyMap _propertyMap;

    public MemberOptions(PropertyMap propertyMap)
    {
        _propertyMap = propertyMap;
    }

    public void MapFrom(Func<TSource, object> mapFunc)
    {
        _propertyMap.CustomMapping = src => mapFunc((TSource)src);
    }

    public void Ignore()
    {
        _propertyMap.Ignore = true;
    }

    public void Condition(Func<TSource, bool> condition)
    {
        _propertyMap.Condition = src => condition((TSource)src);
    }
}
