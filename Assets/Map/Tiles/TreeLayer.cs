using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Map
{

public class TreeLayer : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Tree")]
    [SerializeField] private Texture2D treeTexture;
    [Space] 
    [SerializeField] private bool useMarching = true;
    [SerializeField] private TileMarchingSet treeMarching;
    [SerializeField] private TileBase cutoutTile;
    [Space] 
    [SerializeField] private TileMarchingSet outlineMarching;

    private Tilemap _treeMap;
    private Tilemap _outlineMap;
    private Datamap<bool> _placed;

    private Material _baseMaterial;
    private Material _worldMaterial;
    private float _fogScale = 0f;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(treeTexture);
        if (useMarching) Assert.IsNotNull(treeMarching);
        else Assert.IsNotNull(cutoutTile);
        Assert.IsNotNull(outlineMarching);

        treeMarching.ParseTiles();
        outlineMarching.ParseTiles();

        _baseMaterial = new Material(GlobalConfig.Ins.StandardMaterial);
        _baseMaterial.SetFloat(RockUtil.FogIntensityID, _fogScale);
        
        _worldMaterial = new Material(GlobalConfig.Ins.WorldTextureMaskMaterial);
        _worldMaterial.SetFloat(RockUtil.FogIntensityID, _fogScale);
        _worldMaterial.SetTexture(RockUtil.WorldTextureID, treeTexture);
        
        _treeMap = RockUtil.CreateTilemap(Target, 0, "Tree Tilemap");
        _outlineMap = RockUtil.CreateTilemap(Target, 1, "Tree Outline Tilemap");
        
        _treeMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _worldMaterial;
        _outlineMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _baseMaterial;

        _treeMap.gameObject.AddComponent<TilemapCollider2D>();
        _treeMap.gameObject.layer = 8;
    }

    public override void Initialise()
    {
        _placed = new Datamap<bool>(Space.MapSize, false);
    }

    public override void Activate() { }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    // public override bool CheckOverlap(Vector2 pos)
    // {
    //     if (!Space.SnapWorldToMap(pos, out var mapPos)) return false;
    //     return _placed.At(mapPos);
    // }

    public override string JsonName => "treeLayer";
    public override IEnumerator<PropertyHandle> GetProperties()
    {   
        yield return new PropertyHandle()
        {
            PropertyName = "Fog Intensity %",
            PropertyType = PropertyType.Decimal,
            Getter = () => _fogScale * 100f,
            Setter = (value) =>
            {
                _fogScale = (float)value / 100f;
                _baseMaterial.SetFloat(RockUtil.FogIntensityID, _fogScale);
                _worldMaterial.SetFloat(RockUtil.FogIntensityID, _fogScale);
            }
        };
    }
    
    public override JSONNode ExtractData()
    {
        var json = new JSONObject {
            ["placed"] = _placed.ExtractData(),
            ["fogScale"] = _fogScale
        };
        return json;
    }

    public override void Replicate(JSONNode data)
    {
        var placedPacked = data["placed"];
        _fogScale = data["fogScale"].AsFloat;
        
        _placed.Replicate(placedPacked);

        for (var x = 0; x < _placed.Width; x++)
        for (var y = 0; y < _placed.Height; y++)
            UpdateVisualsAt(new Vector2Int(x, y));
    }


    public void ChangeAt(Vector2 rootWorldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(rootWorldPos, out var rootPos)
            || shouldPlaceNotRemove == _placed.At(rootPos)) return;

        _placed.At(rootPos) = shouldPlaceNotRemove;

        foreach (var subPos in Space.RetrievePositions(rootPos, RockUtil.FullAreaOffsets))
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
            var fullQuery = new MarchingTileQuery(new bool[RockUtil.FullNeighbourOffsets.Length]);
            var halfQuery = new MarchingTileQuery(new bool[RockUtil.HalfNeighbourOffsets.Length]);
            for (var i = 0; i < RockUtil.FullNeighbourOffsets.Length; i++)
            {
                var n = pos + RockUtil.FullNeighbourOffsets[i];
                var bounded = Space.IsInBounds(n);
                var check = (!bounded && placedHere) || (bounded && _placed.At(n));
                fullQuery.Neighbours[i] = check;
                if (i < RockUtil.HalfNeighbourOffsets.Length)
                    halfQuery.Neighbours[i] = check;
            }

            var gotTile = usedSet.TryGetTile(fullQuery, out var variants) ||
                          usedSet.TryGetTile(halfQuery, out variants);
            treeTile = gotTile ? variants[Random.Range(0, variants.Length)] : null;
        }

        usedMap.SetTile((Vector3Int)pos, treeTile);
        otherMap.SetTile((Vector3Int)pos, null);
    }
}

}