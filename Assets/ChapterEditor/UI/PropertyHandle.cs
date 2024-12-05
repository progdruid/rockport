using System;

namespace ChapterEditor
{

public enum PropertyType
{
    Decimal,
    Integer,
    Text
}

public struct PropertyHandle
{
    public string PropertyName;
    public PropertyType PropertyType;

    public Action<object> Setter;
    public Func<object> Getter;
}

}