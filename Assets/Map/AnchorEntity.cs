using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{
public class AnchorEntity : MapEntity, IReplicable
{
    public override float GetReferenceZ() => Target.position.z;

    public Vector2Int GetPos()
    {
        var res = Space.SnapWorldToMap(Target.position, out var mapPos);
        Assert.IsTrue(res);
        return mapPos;
    }

    public override JSONObject ExtractData()
    {
        var json = new JSONObject();
        json["mapPos"] = GetPos().ToJson();
        return json;
    }

    public override void Replicate(JSONObject data)
    {
        var mapPos = data["mapPos"].ReadVector2Int();
        var worldPos = Space.ConvertMapToWorld(mapPos);
        Target.position = new Vector3(worldPos.x, worldPos.y, Target.position.z);

        RequestInitialise();
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }
}
}