using UnityEngine;
using UnityEngine.Tilemaps;

public static class RockUtil
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
        RockUtil.IsInBounds(point, renderer.bounds.min, renderer.bounds.max);
    
    
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
    
    
    public static bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    
    
    public static void DrawDebugPoint(Vector3 pos, Color color, float size = 0.02f, float duration = float.MaxValue)
    {
        Debug.DrawLine(pos + new Vector3(-1, -1, 0) * size, pos + new Vector3( 1, 1, 0) * size, color, duration);
        Debug.DrawLine(pos + new Vector3( 1, -1, 0) * size, pos + new Vector3(-1, 1, 0) * size, color, duration);
    }
    
    public static void DrawDebugCapsule(Vector2 position, Vector2 size, Color color, float duration = float.MaxValue)
    {
        float radius = size.x * 0.5f;
        float height = size.y;
        int segments = 12;
        Vector2 up = Vector2.up * (height * 0.5f - radius);
        Vector2 down = -up;
        Vector2 right = Vector2.right * radius;

        Debug.DrawLine(position + up + right, position + down + right, color, duration);
        Debug.DrawLine(position + up - right, position + down - right, color, duration);

        void DrawArc(Vector2 center, float startAngle, float endAngle)
        {
            float angleStep = (endAngle - startAngle) / segments;
            Vector2 prevPoint = center + new Vector2(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad)) * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 newPoint = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                Debug.DrawLine(prevPoint, newPoint, color, duration);
                prevPoint = newPoint;
            }
        }

        DrawArc(position + up, 0, 180);
        DrawArc(position + down, 180, 360);
    }
}