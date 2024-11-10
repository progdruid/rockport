using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelSpaceHolder : MonoBehaviour
{
    [SerializeField] private Vector2Int size;
    [SerializeField] private Grid visualGrid;
    
    public Vector2Int Size => size;
    
    private void Awake()
    {
        Assert.IsNotNull(visualGrid);
        Assert.IsTrue(size.x > 0);
        Assert.IsTrue(size.y > 0);
    }
    
    public bool ConvertWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
    {
        var origin = visualGrid.transform.position;
        mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)origin) / visualGrid.cellSize);
        return new Rect(0, 0, size.x - 0.1f, size.y - 0.1f).Contains(mapPos);
    }
    
    public IEnumerable<Vector2Int> RetrievePositions(Vector2Int pos, IEnumerable<Vector2Int> offsets)
    {
        foreach (var direction in offsets)
        {
            var neighbour = pos + direction;
            if (neighbour.x >= 0 && neighbour.x < Size.x && neighbour.y >= 0 && neighbour.y < Size.y)
                yield return neighbour;
        }
    }

    public bool IsInBounds(Vector2Int pos) 
        => pos.x >= 0 && pos.x < Size.x && pos.y >= 0 && pos.y < Size.y;

    public Tilemap CreateTilemap(int layer, int offset, string mapName)
    {
        var go = new GameObject(mapName);
        go.transform.SetParent(visualGrid.transform, false);
        go.transform.localPosition = Vector3.back * (layer + 0.01f * offset);
        var map = go.AddComponent<Tilemap>();
        map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        map.orientation = Tilemap.Orientation.XY;
        return map;
    }
}
