
using System;
using System.Collections.Generic;

namespace Map
{

public interface IPropertyHolder
{
    public event Action PropertiesChangeEvent;
    public abstract IEnumerator<PropertyHandle> GetProperties();
}

}