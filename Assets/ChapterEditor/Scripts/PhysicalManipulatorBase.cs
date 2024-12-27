using System.Collections.Generic;

namespace ChapterEditor
{

public abstract class PhysicalManipulatorBase : ManipulatorBase
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool _generatePhysics;

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;
        
        yield return new PropertyHandle()
        {
            PropertyName = "Generate Physics",
            PropertyType = PropertyType.Text,
            Getter = () => _generatePhysics ? "true" : "false",
            Setter = (object input) => _generatePhysics = (string)input == "true"
        };
    }

    public override string Pack() => _generatePhysics ? "true" : "false";

    public override void Unpack(string data)
    {
        _generatePhysics = data == "true";
        InvokePropertiesChangeEvent();
    }

    public override void KillDrop()
    {
        if (_generatePhysics)
            GeneratePhysics();
        Destroy(this);
    }
    
    protected virtual void GeneratePhysics() { }
}

}