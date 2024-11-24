
using System;
using UnityEngine.Events;

public enum TextPropertyType
{
    Decimal,
    Integer,
    Text
}

public struct TextPropertyHandle
{
    public string PropertyName;
    public string PropertyDefaultValue;
    public TextPropertyType TextPropertyType;
    
    public UnityAction<string> ParserSetter;
    public UnityEvent<string> ChangeEvent;
}