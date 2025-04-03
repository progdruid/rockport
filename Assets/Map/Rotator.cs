using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Map
{

public class Rotator : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private float _angle = 0;
    private string _direction = null;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake() {}
    public override void Initialise() { }
    public override void Activate() { }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "rotator";
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Facing",
            PropertyType = PropertyType.Text,
            Getter = () => _angle,
            Setter = (value) =>
            {
                var text = (string)value;
                text = text.ToLowerInvariant();
                _angle = text switch 
                {
                    "up" => 0,
                    "left" => 90,
                    "down" => 180,
                    "right" => 270,
                    _ => _angle
                };
                
                Target.rotation = Quaternion.Euler(0, 0, _angle);
            }
        };
    }

    public override void Replicate(JSONNode data)
    {
        _angle = data["angle"].AsFloat;
        Target.rotation = Quaternion.Euler(0, 0, _angle);
        InvokePropertiesChangeEvent();
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject
        {
            ["angle"] = _angle
        };
        return json;
    }
}

}