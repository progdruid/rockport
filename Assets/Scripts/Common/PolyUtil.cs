using UnityEngine;

public static class PolyUtil
{
    public static readonly Vector2Int[] NeighbourOffsets =
    {
        new(-1, 1), new(0, 1), new(1, 1),
        new(-1, 0), /*0, 0*/ new(1, 0),
        new(-1, -1), new(0, -1), new(1, -1)
    };

    public static bool IsInBounds(Vector2Int point, Vector2Int start, Vector2Int end) =>
        point.x >= start.x && point.x < end.x && point.y >= start.y && point.y < end.y;
}