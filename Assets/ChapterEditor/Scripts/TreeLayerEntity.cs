using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ChapterEditor
{

public class TreeLayerEntity : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

    private PhysicalEntityTrait _physicalTrait;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();

        Assert.IsNotNull(treeTexture);
        if (useMarching) Assert.IsNotNull(treeMarching);
        else Assert.IsNotNull(cutoutTile);
        Assert.IsNotNull(outlineMarching);

        treeMarching.ParseTiles();
        outlineMarching.ParseTiles();

        _baseMaterial = new Material(GlobalConfig.Ins.StandardMaterial);
        _baseMaterial.SetFloat(Lytil.FogIntensityID, _fogScale);
        
        _worldMaterial = new Material(GlobalConfig.Ins.WorldTextureMaskMaterial);
        _worldMaterial.SetFloat(Lytil.FogIntensityID, _fogScale);
        _worldMaterial.SetTexture(Lytil.WorldTextureID, treeTexture);
        
        _treeMap = Lytil.CreateTilemap(Target, 0, "Tree Tilemap");
        _outlineMap = Lytil.CreateTilemap(Target, 1, "Tree Outline Tilemap");
        
        _treeMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _worldMaterial;
        _outlineMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _baseMaterial;
        
        _physicalTrait = new PhysicalEntityTrait();
        _physicalTrait.AddTilemap(_treeMap);
        _physicalTrait.PropertiesChangeEvent += InvokePropertiesChangeEvent;
    }

    protected override void Initialise()
    {
        _placed = new Datamap<bool>(Space.MapSize, false);
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override float GetReferenceZ() => _treeMap.transform.position.z;
    public override bool CheckOverlap(Vector2 pos)
    {
        if (!Space.SnapWorldToMap(pos, out var mapPos)) return false;
        return _placed.At(mapPos);
    }

    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;

        var physicalTraitIter = _physicalTrait.GetProperties();
        while (physicalTraitIter.MoveNext())
            yield return physicalTraitIter.Current;
        
        yield return new PropertyHandle()
        {
            PropertyName = "Fog Intensity %",
            PropertyType = PropertyType.Decimal,
            Getter = () => _fogScale * 100f,
            Setter = (value) =>
            {
                _fogScale = (float)value / 100f;
                _baseMaterial.SetFloat(Lytil.FogIntensityID, _fogScale);
                _worldMaterial.SetFloat(Lytil.FogIntensityID, _fogScale);
            }
        };
    }
    
    public override string Pack() => JsonUtility.ToJson((_physicalTrait.Pack(), _placed.Pack()));

    public override void Unpack(string data)
    {
        var (physicalPacked, placedPacked) = JsonUtility.FromJson<(string, string)>(data);
        
        RequestInitialise();
        _physicalTrait.Unpack(physicalPacked);
        _placed.Unpack(placedPacked);
        
        for (var x = 0; x < _placed.Width; x++)
        for (var y = 0; y < _placed.Height; y++)
            UpdateVisualsAt(new Vector2Int(x, y));
    }


    public override void ChangeAt(Vector2 rootWorldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(rootWorldPos, out var rootPos)
            || shouldPlaceNotRemove == _placed.At(rootPos)) return;

        _placed.At(rootPos) = shouldPlaceNotRemove;

        foreach (var subPos in Space.RetrievePositions(rootPos, Lytil.FullAreaOffsets))
            UpdateVisualsAt(subPos);
    }

    public override void Activate() => _physicalTrait.RequestGeneratePhysics();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateVisualsAt(Vector2Int pos)
    {
        var placedHere = _placed.At(pos);

        var usedSet = placedHere ? treeMarching : outlineMarching;
        var (usedMap, otherMap) = placedHere ? (_treeMap, _outlineMap) : (_outlineMap, _treeMap);

        var treeTile = cutoutTile;

        if (useMarching || !placedHere)
        {
            var fullQuery = new MarchingTileQuery(new bool[Lytil.FullNeighbourOffsets.Length]);
            var halfQuery = new MarchingTileQuery(new bool[Lytil.HalfNeighbourOffsets.Length]);
            for (var i = 0; i < Lytil.FullNeighbourOffsets.Length; i++)
            {
                var n = pos + Lytil.FullNeighbourOffsets[i];
                var bounded = Space.IsInBounds(n);
                var check = (!bounded && placedHere) || (bounded && _placed.At(n));
                fullQuery.Neighbours[i] = check;
                if (i < Lytil.HalfNeighbourOffsets.Length)
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