using System.Collections.Generic;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TreeManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    //static part///////////////////////////////////////////////////////////////////////////////////////////////////////
    private static readonly int WorldTextureShaderPropertyID = Shader.PropertyToID("_WorldTex");
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
    private Datamap<bool> _placed;

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

    protected override void Initialise()
    {
        _placed = new Datamap<bool>(holder.TileSize, false);
    }
    

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override float GetReferenceZ() => _treeMap.transform.position.z;
    public override string SerializeData() => _placed.SerializeData();
    public override void DeserializeData(string data)
    {
        RequestInitialise();
        _placed.DeserializeData(data);
        for (var x = 0; x < _placed.Width; x++)
        for (var y = 0; y < _placed.Height; y++)
            UpdateVisualsAt(new Vector2Int(x, y));
    }

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

        _placed.At(rootPos) = shouldPlaceNotRemove;
        
        foreach (var subPos in holder.RetrievePositions(rootPos, PolyUtil.FullAreaOffsets)) 
            UpdateVisualsAt(subPos);
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateVisualsAt(Vector2Int pos)
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
