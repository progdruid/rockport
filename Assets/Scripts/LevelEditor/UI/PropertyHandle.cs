using UnityEngine.Events;

public enum PropertyType
{
    Decimal,
    Integer,
    Text
}

public struct PropertyHandle
{
    public string PropertyName;
    public string PropertyDefaultValue;
    public PropertyType PropertyType;
    
    public UnityAction<object> Setter;
    public UnityEvent<object> ChangeEvent;
}