using System;
using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class TreeManipulator : MonoBehaviour
{
    [SerializeField] private LevelSpaceHolder holder;
    [SerializeField] private TileBase basePlaceholderTile;
    [SerializeField] private Material treeBaseMaterial;
    [SerializeField] private TileMarchingSet inlineMarching;
    
    private Tilemap _baseMap;
    private Tilemap _marchingMap;
    private bool[,] _placed;
    
    #region Engine

    private void Awake()
    {
        Assert.IsNotNull(holder);
        Assert.IsNotNull(basePlaceholderTile);
        Assert.IsNotNull(treeBaseMaterial);
        Assert.IsNotNull(inlineMarching);
        
        inlineMarching.ParseTiles();
    }

    private void Start()
    {
        _placed = new bool[holder.Size.x, holder.Size.y];
        
        _baseMap = holder.CreateTilemap(-2, "Tree Base Tilemap");
        var basemapRenderer = _baseMap.gameObject.AddComponent<TilemapRenderer>();
        basemapRenderer.sharedMaterial = treeBaseMaterial;
        
        _marchingMap = holder.CreateTilemap(-1, "Tree Marching Tilemap");
        var marchmapRenderer = _marchingMap.gameObject.AddComponent<TilemapRenderer>();
    }

    #endregion
    
    #region Private Logic

    private void UpdateMarchingFor(Vector2Int pos)
    {
        if (!_placed.At(pos))
        {
            _marchingMap.SetTile((Vector3Int)pos, null);
            return;
        }
        
        var fullQuery = new MarchingTileQuery(new bool[PolyUtil.FullNeighbourOffsets.Length]);
        var halfQuery = new MarchingTileQuery(new bool[PolyUtil.HalfNeighbourOffsets.Length]);
        for (var i = 0; i < PolyUtil.FullNeighbourOffsets.Length; i++)
        {
            var n = pos + PolyUtil.FullNeighbourOffsets[i];
            var check = !holder.IsInBounds(n) || _placed.At(n);
            fullQuery.Neighbours[i] = check;
            if (i < PolyUtil.HalfNeighbourOffsets.Length)
                halfQuery.Neighbours[i] = check;
        }

        var marchingTile =
            (inlineMarching.TryGetTile(fullQuery, out var variants) ||
             inlineMarching.TryGetTile(halfQuery, out variants))
                ? variants[UnityEngine.Random.Range(0, variants.Length)]
                : null;
        
        _marchingMap.SetTile((Vector3Int)pos, marchingTile);
    }
    
    private void ChangeTreeTile(Vector2Int pos, bool place)
    {
        if (place == _placed.At(pos))
            return;

        _placed.Set(pos, place);
        _baseMap.SetTile((Vector3Int)pos, place ? basePlaceholderTile : null);

        foreach (var subPos in holder.RetrievePositions(pos, PolyUtil.FullAreaOffsets)) 
            UpdateMarchingFor(subPos);
    }
    
    #endregion
    
    #region Public Interface

    public void ChangeTileAtWorldPos(Vector2 worldPos, bool place)
    {
        var inBounds = holder.ConvertWorldToMap(worldPos, out var mapPos);
        if (!inBounds) return;

        ChangeTreeTile(mapPos, place);
    }

    #endregion
}
