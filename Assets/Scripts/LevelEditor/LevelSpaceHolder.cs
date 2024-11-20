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

    public int ManipulatorsCount => _manipulators.Count;
    
    private List<ManipulatorBase> _manipulators;
    
    private void Awake()
    {
        Assert.IsNotNull(visualGrid);
        Assert.IsTrue(size.x > 0);
        Assert.IsTrue(size.y > 0);
        
        _manipulators = new();
    }

    public ManipulatorBase GetManipulator(int layer) => _manipulators[layer];
    public bool HasLayer(int layer) => layer >= 0 && layer < ManipulatorsCount;
    
    public bool RegisterObject(ManipulatorBase manipulator, out int layer)
    {
        layer = -1;
        if (_manipulators.Contains(manipulator))
            return false;
        
        _manipulators.Add(manipulator);
        layer = _manipulators.Count - 1;
        manipulator.InjectHolder(this);
        
        manipulator.Target.SetParent(visualGrid.transform);
        UpdateZ(layer);
        
        return true;
    }

    public bool RegisterAt(ManipulatorBase manipulator, int layer)
    {
        var to = Mathf.Clamp(layer, 0, _manipulators.Count);
        var registered = RegisterObject(manipulator, out var from);
        if (registered)
            MoveRegister(from, to);
        return registered;
    }

    public bool UnregisterObject(ManipulatorBase manipulator)
    {
        var layer = _manipulators.BinarySearch(manipulator);
        if (layer < 0) return false;

        manipulator.InjectHolder(null);
        manipulator.Target.SetParent(null);
        _manipulators.RemoveAt(layer);
        for (var i = layer; i < _manipulators.Count - 1; i++)
            UpdateZ(i);
        return true;
    }

    public void MoveRegister(int layerFrom, int layerTo)
    {
        layerTo = Mathf.Clamp(layerTo, 0, _manipulators.Count - 1);
        if (layerFrom < 0 || layerFrom >= _manipulators.Count || layerFrom == layerTo) return;
        
        var movedObject = _manipulators[layerFrom];
        _manipulators.RemoveAt(layerFrom);
        _manipulators.Insert(layerTo, movedObject);
        
        var step = layerTo > layerFrom ? 1 : -1;
        for (var i = layerFrom; i != layerTo+step; i += step) 
            UpdateZ(i);
    }

    private void UpdateZ(int layer)
    {
        var manipulator = _manipulators[layer];
        var local = manipulator.Target.localPosition;
        manipulator.Target.localPosition = new Vector3(local.x, local.y, -1 * layer);
    }
    
    public bool SnapWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
    {
        mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)visualGrid.transform.position) / visualGrid.cellSize);
        return new Rect(0, 0, size.x - 0.1f, size.y - 0.1f).Contains(mapPos);
    }

    public Vector2 ConvertMapToWorld(Vector2Int mapPos) 
        => (Vector2)visualGrid.transform.position + mapPos * (Vector2)visualGrid.cellSize;

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
}
