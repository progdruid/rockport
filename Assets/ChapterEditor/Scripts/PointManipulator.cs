using System.Drawing;
using UnityEngine;

namespace ChapterEditor
{

public class PointManipulator : ManipulatorBase
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Vector2Int Point { get; private set; } = Vector2Int.zero;

    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override float GetReferenceZ() => Target.position.z;
    
    public override string Pack() => $"{Point.x} {Point.y}";

    public override void Unpack(string data)
    {
        var parts = data.Split(' ');
        Point = Vector2Int.zero;
        if (parts.Length == 2 && int.TryParse(parts[0], out var pointX)  && int.TryParse(parts[1], out var pointY))
            Point = new Vector2Int(pointX, pointY);
        
        RequestInitialise();
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Holder.SnapWorldToMap(worldPos, out var snappedPos)) return;
        Point = snappedPos;
        Debug.Log($"Point set to {Point}");
    }
}

}