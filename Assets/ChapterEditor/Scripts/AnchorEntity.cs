using System.Drawing;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class AnchorEntity : MapEntity
{
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override float GetReferenceZ() => Target.position.z;

    public Vector2Int GetPos()
    {
        var res = Space.SnapWorldToMap(Target.position, out var mapPos);
        Assert.IsTrue(res);
        return mapPos;
    }

    public override string Pack()
    {
        var mapPos = GetPos();
        return $"{mapPos.x} {mapPos.y}";
    }

    public override void Unpack(string data)
    {
        var parts = data.Split(' ');
        var mapPos = Vector2Int.zero;
        if (parts.Length == 2 && int.TryParse(parts[0], out var pointX) && int.TryParse(parts[1], out var pointY))
            mapPos = new Vector2Int(pointX, pointY);
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