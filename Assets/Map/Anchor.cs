using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class Anchor : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake() { }
    public override void Initialise() {}
    public override void Activate() {}

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
        var json = new JSONObject() {
            ["mapPos"] = mapPos.ToJson()
        };
        return json;
    }
    
    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }
}

}