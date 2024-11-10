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
    
    private List<Transform> _layeredObjects;
    
    private void Awake()
    {
        Assert.IsNotNull(visualGrid);
        Assert.IsTrue(size.x > 0);
        Assert.IsTrue(size.y > 0);
        
        _layeredObjects = new();
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

    public Tilemap CreateTilemap(Transform parent, int offset, string mapName)
    {
        var go = new GameObject(mapName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.back * 0.01f * offset;
        var map = go.AddComponent<Tilemap>();
        map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        map.orientation = Tilemap.Orientation.XY;
        return map;
    }
    
    public bool RegisterObject(Transform t, out int layer)
    {
        layer = -1;
        if (_layeredObjects.Contains(t))
            return false;
        
        _layeredObjects.Add(t);
        layer = _layeredObjects.Count - 1;
        
        t.SetParent(visualGrid.transform);
        UpdateZ(layer);
        
        return true;
    }

    public bool RegisterAt(Transform t, int layer)
    {
        var to = Mathf.Clamp(layer, 0, _layeredObjects.Count);
        var registered = RegisterObject(t, out var from);
        if (registered)
            MoveFromTo(from, to);
        return registered;
    }

    public bool UnregisterObject(Transform t)
    {
        var layer = _layeredObjects.BinarySearch(t);
        if (layer < 0) return false;
        
        _layeredObjects[layer].SetParent(null);
        _layeredObjects.RemoveAt(layer);
        for (var i = layer; i < _layeredObjects.Count - 1; i++)
            UpdateZ(i);
        return true;
    }

    public void MoveFromTo(int layerFrom, int layerTo)
    {
        layerTo = Mathf.Clamp(layerTo, 0, _layeredObjects.Count - 1);
        if (layerFrom < 0 || layerFrom >= _layeredObjects.Count || layerFrom == layerTo) return;
        
        var movedObject = _layeredObjects[layerFrom];
        _layeredObjects.RemoveAt(layerFrom);
        _layeredObjects.Insert(layerTo, movedObject);
        
        var step = layerTo > layerFrom ? 1 : -1;
        for (var i = layerFrom; i != layerTo+step; i += step) 
            UpdateZ(i);
    }

    private void UpdateZ(int layer)
    {
        var t = _layeredObjects[layer];
        var local = t.localPosition;
        t.localPosition = new Vector3(local.x, local.y, -1 * layer);
    }
}
