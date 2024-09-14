using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelHolder : MonoBehaviour
{
    [SerializeField] private Vector2Int mapSize;
    [Space] 
    [SerializeField] private Grid visualGrid;
    [SerializeField] private SerializableMap<string, Tilemap> associatedVisualMaps;
    [SerializeField] private LevelPass[] passes;
    
    private Dictionary<string, Datamap> _datamaps = new ();
    
    private void Start()
    {
        foreach (var pass in passes)
            pass.InjectLevelHolder(this);
    }

    public Datamap ObtainDatamap<T>(string id) => ObtainDatamap<T>(id, out _);
    public Datamap ObtainDatamap<T>(string id, out bool wasCreated)
    {
        wasCreated = !_datamaps.TryGetValue(id, out var datamap);
        if (!wasCreated)
            return datamap;

        var map = new Datamap(mapSize, Activator.CreateInstance<T>());
        _datamaps.Add(id, map);
        return map;
    }


    public void ChangeVisualAt(string tilemapId, Vector2Int blockPos, TileBase tile)
    {
        GetVisualMap(tilemapId, out var map);
        ChangeVisualAt(map, blockPos, tile);
    }
    
    public void ChangeVisualAt(Tilemap map, Vector2Int blockPos, TileBase tile)
    {
        var visualPos = (Vector3Int)(blockPos - mapSize / 2);
        map.SetTile(visualPos, tile);
    }
    
    public Vector2Int ConvertWorldToMapPos(Vector2 worldPos) => Vector2Int.FloorToInt(worldPos / visualGrid.cellSize) + mapSize / 2;
    public bool GetVisualMap(string id, out Tilemap map) => associatedVisualMaps.TryGetValue(id, out map);
    public Vector3 GetOrigin() => visualGrid.gameObject.transform.position;
    public Vector2Int GetMapSize() => mapSize;

}
