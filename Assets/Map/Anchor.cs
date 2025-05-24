using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class Anchor : EntityComponent, IAnchorAccessor
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private bool freeMode = false;
    
    MapEntity IEntityAccessor.Entity { get; set; }
    string IEntityAccessor.AccessorName { get; set; }

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake() => Entity.AddAccessor("anchor", this);
    public override void Initialise() { }
    public override void Activate() { }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "anchor";

    public override IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Free Mode",
            PropertyType = PropertyType.Text,
            Getter = () => freeMode ? "true" : "false",
            Setter = (object input) =>
            {
                freeMode = (string)input == "true";
                InvokePropertiesChangeEvent();
            }
        };
    }

    public override void Replicate(JSONNode data)
    {
        if (data["freeMode"] != null)
            freeMode = data["freeMode"].AsBool;
        var pos = data["mapPos"].ReadVector2();
        var world = Space.ConvertMapToWorld(pos);
        Target.SetWorldXY(world);
    }

    public override JSONNode ExtractData()
    {
        Space.ConvertWorldToMapAbsolute(Target.position.To2(), out var mapPos);
        var json = new JSONObject()
        {
            ["mapPos"] = mapPos.ToJson(),
            ["freeMode"] = freeMode
        };
        return json;
    }

    public bool SetPosition(Vector2 worldPos)
    {
        if (!freeMode)
        {
            if (!Space.SnapWorldToMap(worldPos, out var snapped))
                return false;
            worldPos = Space.ConvertMapToWorld(snapped);
        }
        else if (!Space.ConvertWorldToMapAbsolute(worldPos, out _))
            return false;
        
        Target.SetWorldXY(worldPos);
        return true;
    }

    public Vector2 GetPosition() => Target.position.To2();
}

public interface IAnchorAccessor : IEntityAccessor
{
    public bool SetPosition(Vector2 worldPos);
    public Vector2 GetPosition();
}

}