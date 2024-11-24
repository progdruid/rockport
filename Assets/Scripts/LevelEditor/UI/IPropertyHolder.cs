
using System.Collections.Generic;

public interface IPropertyHolder
{
    public abstract IEnumerator<PropertyHandle> GetProperties();
}
