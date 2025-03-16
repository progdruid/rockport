using System;

namespace Map
{

public enum PropertyType
{
    Decimal,
    Integer,
    Text,
    Bool
}

public struct PropertyHandle
{
    public string PropertyName;
    public PropertyType PropertyType;

    public Action<object> Setter;
    public Func<object> Getter;
}

}