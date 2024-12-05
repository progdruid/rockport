
using System;
using System.Collections.Generic;

namespace ChapterEditor
{

public interface IPropertyHolder
{
    public event Action PropertiesChangeEvent;
    public abstract IEnumerator<PropertyHandle> GetProperties();
}

}