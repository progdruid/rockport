using UnityEngine;
using UnityEngine.Tilemaps;

public static class Lytil
{
    public static readonly int FogColorID = Shader.PropertyToID("_FogColor");
    public static readonly int FogIntensityID = Shader.PropertyToID("_FogIntensity");
    public static readonly int WorldTextureID = Shader.PropertyToID("_WorldTex");
    
    public static readonly Vector2Int[] HalfNeighbourOffsets =
    {
        new( 0, 1), new(1, 0), new(0, -1), new(-1,  0)
    };
    
    public static readonly Vector2Int[] FullNeighbourOffsets =
    {
        new( 0, 1), new(1, 0), new(0, -1), new(-1,  0),
        new(-1, 1), new(1, 1), new(1, -1), new(-1, -1)
    };
    
    public static readonly Vector2Int[] FullAreaOffsets =
    {
        new( 0, 1), new(1, 0), new(0, -1), new(-1,  0),
        new(-1, 1), new(1, 1), new(1, -1), new(-1, -1),
        new( 0, 0)
    };

    
    public static bool IsInBounds(Vector2Int point, Vector2Int start, Vector2Int end) =>
        point.x >= start.x && point.x < end.x && point.y >= start.y && point.y < end.y;
    
    public static bool IsInBounds(Vector2 point, Vector2 start, Vector2 end) =>
        point.x >= start.x && point.x < end.x && point.y >= start.y && point.y < end.y;

    public static bool IsInRendererBounds(Vector2 point, SpriteRenderer renderer) =>
        Lytil.IsInBounds(point, renderer.bounds.min, renderer.bounds.max);
    
    
    public static Tilemap CreateTilemap(Transform parent, int offset, string mapName)
    {
        var go = new GameObject(mapName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.back * 0.01f * offset;
        var map = go.AddComponent<Tilemap>();
        map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        map.orientation = Tilemap.Orientation.XY;
        return map;
    }
    
    public static string PackVector2Int (Vector2Int vector) => $"{vector.x}, {vector.y}";

    public static Vector2Int UnpackVector2Int(string data)
    {
        var split = data.Split(',');
        return new Vector2Int (int.Parse(split[0]), int.Parse(split[1]));
    }
    
}