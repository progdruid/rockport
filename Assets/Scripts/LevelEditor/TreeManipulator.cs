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
    //static part///////////////////////////////////////////////////////////////////////////////////////////////////////
    private static readonly int WorldTextureShaderPropertyID = Shader.PropertyToID("_WorldTex");
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        Assert.IsNotNull(treeTexture);
        Assert.IsNotNull(worldTextureCutoutMaterial);
        if (useMarching) Assert.IsNotNull(treeMarching);
        else Assert.IsNotNull(cutoutTile);
        Assert.IsNotNull(outlineMarching);
        
        treeMarching.ParseTiles();
        outlineMarching.ParseTiles();
                
        _treeMap = CreateTilemap(0, "Tree Tilemap");
        var marchMapRenderer = _treeMap.gameObject.AddComponent<TilemapRenderer>();
        var mat = new Material(worldTextureCutoutMaterial);
        mat.SetTexture(WorldTextureShaderPropertyID, treeTexture);
        marchMapRenderer.sharedMaterial = mat;

        _outlineMap = CreateTilemap(1, "Tree Outline Tilemap");
        _outlineMap.gameObject.AddComponent<TilemapRenderer>();
    }

    private void Start()
    {
        _placed = new bool[holder.Size.x, holder.Size.y];
        holder.RegisterAt(this, layer);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public float GetZForInteraction() => _treeMap.transform.position.z;
    
    public override void SubscribeInput(EditorController controller)
    {
        _controller = controller;
        controller.SetPlaceRemoveHandler(this);
        controller.SetPropertyHolder(this);
    }

    public override void UnsubscribeInput()
    {
        if (!_controller) return;
        _controller.UnsetPropertyHolder();
        _controller.UnsetPlaceRemoveHandler();
        _controller = null;
    }
    
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while(iter.MoveNext())
            yield return iter.Current;
    }
    
    public void ChangeAt(Vector2 rootWorldPos, bool shouldPlaceNotRemove)
    {
        if (!holder.SnapWorldToMap(rootWorldPos, out var rootPos) 
            || shouldPlaceNotRemove == _placed.At(rootPos)) return;

        _placed.Set(rootPos, shouldPlaceNotRemove);
        
        foreach (var subPos in holder.RetrievePositions(rootPos, PolyUtil.FullAreaOffsets)) 
            UpdateVisualsFor(subPos);
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
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
