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
    MapEntity IEntityAccessor.Entity { get; set; }
    string IEntityAccessor.AccessorName { get; set; }

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake() => Entity.AddPublicModule("anchor", this);
    public override void Initialise() { }
    public override void Activate() { }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "anchor";
    public override IEnumerator<PropertyHandle> GetProperties() { yield break; }

    public override void Replicate(JSONNode data)
    {
        var pos = data["mapPos"].ReadVector2Int();
        var world = Space.ConvertMapToWorld(pos);
        Target.SetWorldXY(world);
    }

    public override JSONNode ExtractData()
    {
        Space.SnapWorldToMap(Target.position.To2(), out var mapPos);
        var json = new JSONObject()
        {
            ["mapPos"] = mapPos.ToJson()
        };
        return json;
    }

    public bool SetPositionSnapped(Vector2 worldPos)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return false;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
        return true;
    }

    public bool SetPositionAbsolute(Vector2 worldPos)
    {
        if (!Space.IsInBounds(worldPos.RoundToInt())) return false;
        Target.SetWorldXY(worldPos);
        return true;
    }

    public Vector2 GetPosition() => Target.position.To2();
}

public interface IAnchorAccessor : IEntityAccessor
{
    public bool SetPositionSnapped(Vector2 worldPos);
    public bool SetPositionAbsolute(Vector2 worldPos);
    public Vector2 GetPosition();
}

}