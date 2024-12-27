using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class MapSpaceHolder
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private Vector2Int _mapSize;
    private readonly Grid _visualGrid;
    private readonly List<ManipulatorBase> _manipulators;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////    
    public MapSpaceHolder(Vector2Int size)
    {
        Assert.IsTrue(size.x > 0);
        Assert.IsTrue(size.y > 0);
        _mapSize = size;
        var gridObject = new GameObject("LayerSheet");
        _visualGrid = gridObject.AddComponent<Grid>();
        _visualGrid.cellSize = new Vector3(0.5f, 0.5f, 0f);
        _manipulators = new List<ManipulatorBase>();
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public GameObject GameObject => _visualGrid.gameObject;
    public Vector2Int MapSize => _mapSize;
    public Vector2 WorldSize => _mapSize * (Vector2)_visualGrid.cellSize;
    public Vector2 WorldStart => _visualGrid.transform.position;
    public int ManipulatorsCount => _manipulators.Count;
    
    //grid operations
    public bool SnapWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
    {
        mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)_visualGrid.transform.position) / _visualGrid.cellSize);
        return new Rect(0, 0, _mapSize.x - 0.1f, _mapSize.y - 0.1f).Contains(mapPos);
    }
    public Vector2 ConvertMapToWorld(Vector2Int mapPos) 
        => (Vector2)_visualGrid.transform.position + mapPos * (Vector2)_visualGrid.cellSize;
    public bool IsInBounds(Vector2Int pos) => pos.x >= 0 && pos.x < MapSize.x && pos.y >= 0 && pos.y < MapSize.y;
    public IEnumerable<Vector2Int> RetrievePositions(Vector2Int pos, IEnumerable<Vector2Int> offsets)
    {
        foreach (var direction in offsets)
        {
            var neighbour = pos + direction;
            if (neighbour.x >= 0 && neighbour.x < MapSize.x && neighbour.y >= 0 && neighbour.y < MapSize.y)
                yield return neighbour;
        }
    }
    
    //layer operations
    public ManipulatorBase GetManipulator(int layer) => _manipulators[layer];
    public ManipulatorBase GetTopmostManipulator() => _manipulators[^1];
    public bool HasLayer(int layer) => layer >= 0 && layer < ManipulatorsCount;
    public int ClampLayer(int layer) => Mathf.Clamp(layer, 0, ManipulatorsCount - 1);
    public bool MoveLayer(int layerFrom, int layerTo)
    {
        layerTo = ClampLayer(layerTo);
        if (!HasLayer(layerFrom) || layerFrom == layerTo) return false;

        var movedObject = _manipulators[layerFrom];
        _manipulators.RemoveAt(layerFrom);
        _manipulators.Insert(layerTo, movedObject);

        var step = layerTo > layerFrom ? 1 : -1;
        for (var i = layerFrom; i != layerTo + step; i += step)
            UpdateZ(i);

        return true;
    }

    public int FindManipulator(string targetName, out ManipulatorBase result)
    {
        for (var layer = 0; layer < _manipulators.Count; layer++)
        {
            if (_manipulators[layer].ManipulatorName != targetName) continue;
            result = _manipulators[layer];
            return layer;
        }
        
        result = null;
        return -1;
    }
    
    
    //registration
    public bool RegisterObject(ManipulatorBase manipulator, out int layer)
    {
        layer = -1;
        if (_manipulators.Contains(manipulator))
            return false;

        _manipulators.Add(manipulator);
        layer = _manipulators.Count - 1;
        manipulator.InjectHolder(this);

        manipulator.Target.SetParent(_visualGrid.transform);
        UpdateZ(layer);

        return true;
    }
    public bool RegisterAt(ManipulatorBase manipulator, int layer)
    {
        var to = Mathf.Clamp(layer, 0, _manipulators.Count);
        var registered = RegisterObject(manipulator, out var from);
        if (registered)
            MoveLayer(from, to);
        return registered;
    }

    public bool UnregisterObject(ManipulatorBase manipulator)
    {
        var layer = _manipulators.FindIndex(m => m == manipulator);
        return UnregisterAt(layer);
    }
    public bool UnregisterAt(int layer)
    {
        if (!HasLayer(layer)) return false;
        var manipulator = GetManipulator(layer);
        manipulator.InjectHolder(null);
        manipulator.Target.SetParent(null);
        _manipulators.RemoveAt(layer);
        for (var i = layer; i < _manipulators.Count - 1; i++)
            UpdateZ(i);
        return true;
    }

    
    public void Kill()
    {
        GameObject.Destroy(_visualGrid.gameObject);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateZ(int layer)
    {
        var manipulator = _manipulators[layer];
        var local = manipulator.Target.localPosition;
        manipulator.Target.localPosition = new Vector3(local.x, local.y, -1 * layer);
    }
}

}