
using System;
using System.Collections.Generic;

public interface IPropertyHolder
{
    public event Action PropertiesChangeEvent;
    public abstract IEnumerator<PropertyHandle> GetProperties();
}
