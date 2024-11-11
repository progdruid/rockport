using System;
using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TreeManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    private static readonly int WorldTextureShaderPropertyID = Shader.PropertyToID("_WorldTex");
    
    [SerializeField] private int layer;
    [Space]
    [SerializeField] private Texture2D treeTexture;
    [SerializeField] private Material worldTextureCutoutMaterial;
    [Space]
    [SerializeField] private bool useMarching = true;
    [SerializeField] private TileMarchingSet treeMarching;
    [SerializeField] private TileBase cutoutTile;
    [Space]
    [SerializeField] private TileMarchingSet outlineMarching;
    
    private Tilemap _treeMap;
    private Tilemap _outlineMap;
    private bool[,] _placed;

    private EditorController _controller;
    
    private void Awake()
    {
        Assert.IsNotNull(holder);
        Assert.IsNotNull(treeTexture);
        Assert.IsNotNull(worldTextureCutoutMaterial);
        if (useMarching) Assert.IsNotNull(treeMarching);
        else Assert.IsNotNull(cutoutTile);
        Assert.IsNotNull(outlineMarching);
        
        treeMarching.ParseTiles();
        outlineMarching.ParseTiles();
    }

    private void Start()
    {
        _placed = new bool[holder.Size.x, holder.Size.y];
        
        var parent = (new GameObject("Tree")).transform;
        holder.RegisterAt(this, layer);
        
        _treeMap = CreateTilemap(0, "Tree Tilemap");
        var marchMapRenderer = _treeMap.gameObject.AddComponent<TilemapRenderer>();
        var mat = new Material(worldTextureCutoutMaterial);
        mat.SetTexture(WorldTextureShaderPropertyID, treeTexture);
        marchMapRenderer.sharedMaterial = mat;

        _outlineMap = CreateTilemap(1, "Tree Outline Tilemap");
        _outlineMap.gameObject.AddComponent<TilemapRenderer>();
    }

    
    public void ChangeAt(Vector2Int pos, bool shouldPlaceNotRemove)
    {
        if (shouldPlaceNotRemove == _placed.At(pos))
            return;

        _placed.Set(pos, shouldPlaceNotRemove);
        
        foreach (var subPos in holder.RetrievePositions(pos, PolyUtil.FullAreaOffsets)) 
            UpdateVisualsFor(subPos);
    }

    public float GetZForInteraction() => _treeMap.transform.position.z;
    
    public override void SubscribeInput(EditorController controller)
    {
        _controller = controller;
        controller.SetPlaceRemoveHandler(this);
    }

    public override void UnsubscribeInput()
    {
        if (!_controller) return;
        _controller.UnsetPlaceRemoveHandler();
        _controller = null;
    }
    
    private void UpdateVisualsFor(Vector2Int pos)
    {
        var placedHere = _placed.At(pos);
        
        var usedSet = placedHere ? treeMarching : outlineMarching;
        var (usedMap, otherMap) = placedHere ? (_treeMap, _outlineMap) : (_outlineMap, _treeMap);

        var treeTile = cutoutTile;
        
        if (useMarching || !placedHere)
        {
            var fullQuery = new MarchingTileQuery(new bool[PolyUtil.FullNeighbourOffsets.Length]);
            var halfQuery = new MarchingTileQuery(new bool[PolyUtil.HalfNeighbourOffsets.Length]);
            for (var i = 0; i < PolyUtil.FullNeighbourOffsets.Length; i++)
            {
                var n = pos + PolyUtil.FullNeighbourOffsets[i];
                var bounded = holder.IsInBounds(n);
                var check = (!bounded && placedHere) || (bounded && _placed.At(n));
                fullQuery.Neighbours[i] = check;
                if (i < PolyUtil.HalfNeighbourOffsets.Length)
                    halfQuery.Neighbours[i] = check;
            }
            
            var gotTile = usedSet.TryGetTile(fullQuery, out var variants) || usedSet.TryGetTile(halfQuery, out variants);
            treeTile = gotTile ? variants[Random.Range(0, variants.Length)] : null;
        }
        
        usedMap.SetTile((Vector3Int)pos, treeTile);
        otherMap.SetTile((Vector3Int)pos, null);
    }
}
