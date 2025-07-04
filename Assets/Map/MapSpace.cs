using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class MapSpace
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private Vector2Int _mapSize;
    private readonly Grid _visualGrid;
    private readonly List<MapEntity> _entities;
    private Dictionary<MapEntity, IntReference> _entityToLayerHandleMap;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////    
    public MapSpace(Vector2Int size)
    {
        Assert.IsTrue(size.x > 0);
        Assert.IsTrue(size.y > 0);
        _mapSize = size;
        var gridObject = new GameObject("LayerSheet");
        _visualGrid = gridObject.AddComponent<Grid>();
        _visualGrid.cellSize = new Vector3(0.5f, 0.5f, 0f);
        _entities = new List<MapEntity>();
        _entityToLayerHandleMap = new Dictionary<MapEntity, IntReference>();
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public GameObject GameObject => _visualGrid.gameObject;
    public Vector2Int MapSize => _mapSize;
    public Vector2 WorldSize => _mapSize * (Vector2)_visualGrid.cellSize;
    public Vector2 WorldStart => _visualGrid.transform.position;
    public int EntitiesCount => _entities.Count;
    
    //grid operations
    public bool IsInBounds(Vector2 mapPos) 
        => mapPos.x >= 0 && mapPos.x < MapSize.x && mapPos.y >= 0 && mapPos.y < MapSize.y;
    public bool SnapWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
    {
        mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)_visualGrid.transform.position) / _visualGrid.cellSize);
        return IsInBounds(mapPos);
    }
    public bool ConvertWorldToMapAbsolute(Vector2 worldPos, out Vector2 mapPos)
    {
        mapPos = (worldPos - (Vector2)_visualGrid.transform.position) / _visualGrid.cellSize;
        return IsInBounds(mapPos);
    }
    public Vector2 ConvertMapToWorld(Vector2 mapPos) 
        => (Vector2)_visualGrid.transform.position + mapPos * (Vector2)_visualGrid.cellSize;
    
    public IEnumerable<Vector2Int> RetrievePositions(Vector2Int pos, IEnumerable<Vector2Int> offsets)
    {
        foreach (var direction in offsets)
        {
            var neighbour = pos + direction;
            if (neighbour.x >= 0 && neighbour.x < MapSize.x && neighbour.y >= 0 && neighbour.y < MapSize.y)
                yield return neighbour;
        }
    }
    
    public float GetMapTop()
        => _entities.Count == 0 ? _visualGrid.transform.position.z : _entities[^1].GetReferenceZ();
    

    //layer operations
    public MapEntity GetEntity(int layer) => _entities[layer];
    public bool HasLayer(int layer) => layer >= 0 && layer < EntitiesCount;
    public int ClampLayer(int layer) => Mathf.Clamp(layer, 0, EntitiesCount - 1);
    public bool MoveLayer(int layerFrom, int layerTo)
    {
        layerTo = ClampLayer(layerTo);
        if (!HasLayer(layerFrom) || layerFrom == layerTo) return false;

        var movedObject = _entities[layerFrom];
        _entities.RemoveAt(layerFrom);
        _entities.Insert(layerTo, movedObject);
        
        var step = layerTo > layerFrom ? 1 : -1;
        for (var i = layerFrom; i != layerTo + step; i += step) 
            UpdateLayer(i);

        return true;
    }

    public int FindEntity(string targetName, out MapEntity result)
    {
        for (var layer = 0; layer < _entities.Count; layer++)
        {
            if (_entities[layer].Title != targetName) continue;
            result = _entities[layer];
            return layer;
        }
        
        result = null;
        return -1;
    }
    
    
    //registration
    public bool RegisterObject(MapEntity entity, out int layer)
    {
        layer = -1;
        if (_entities.Contains(entity))
            return false;

        _entities.Add(entity);
        entity.Target.SetParent(_visualGrid.transform);
        
        layer = _entities.Count - 1;
        var layerHandle = entity.InjectMap(this);
        _entityToLayerHandleMap[entity] = layerHandle;
        UpdateLayer(layer);

        return true;
    }
    public bool RegisterAt(MapEntity entity, int layer)
    {
        var to = Mathf.Clamp(layer, 0, _entities.Count);
        var registered = RegisterObject(entity, out var from);
        if (registered)
            MoveLayer(from, to);
        return registered;
    }

    public bool UnregisterObject(MapEntity entity)
    {
        var layer = _entities.FindIndex(e => e == entity);
        return UnregisterAt(layer);
    }
    public bool UnregisterAt(int layer)
    {
        if (!HasLayer(layer)) return false;
        var entity = GetEntity(layer);
        entity.InjectMap(null);
        entity.Target.SetParent(null);
        _entities.RemoveAt(layer);
        _entityToLayerHandleMap.Remove(entity);
        for (var i = layer; i < _entities.Count - 1; i++)
            UpdateLayer(i);
        return true;
    }

    
    public void Kill()
    {
        GameObject.Destroy(_visualGrid.gameObject);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateLayer(int layer)
    {
        var entity = _entities[layer];
        _entityToLayerHandleMap[entity].Value = layer;
        var local = entity.Target.localPosition;
        entity.Target.localPosition = new Vector3(local.x, local.y, -1 * layer);
    }
}

}