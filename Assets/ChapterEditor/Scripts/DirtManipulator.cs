using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ChapterEditor
{

[System.Serializable]
public struct DirtStratum
{
    [SerializeField] public int thickness;

    [Space] [SerializeField] public TileBase baseTile;

    [Space] [Range(0f, 1f)] [SerializeField]
    public float lowerPebbleDensity;

    [SerializeField] public TileBase[] lowerPebbles;

    [Range(0f, 1f)] [SerializeField] public float upperPebbleDensity;
    [SerializeField] public TileBase[] upperPebbles;

    [SerializeField] public TileMarchingSet marchingSet;
}

public class DirtManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private int maxDepth;
    [SerializeField] private TileMarchingSet outlineMarchingSet;
    [SerializeField] private DirtStratum[] strata;

    private Datamap<int> _depthMap;
    private Tilemap _baseMap;
    private Tilemap _lowerPebbleMap;
    private Tilemap _upperPebbleMap;
    private Tilemap _marchingMap;

    private Material _material;
    private float _fogScale = 0f;
    
    private EditorController _controller;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();

        Assert.IsNotNull(outlineMarchingSet);
        Assert.IsNotNull(strata);

        outlineMarchingSet.ParseTiles();

        foreach (var layer in strata)
            if (layer.marchingSet)
                layer.marchingSet.ParseTiles();

        
        _baseMap = CreateTilemap(0, "Dirt Base Tilemap");
        _lowerPebbleMap = CreateTilemap(1, "Dirt Lower Pebble Tilemap");
        _upperPebbleMap = CreateTilemap(2, "Dirt Upper Pebble Tilemap");
        _marchingMap = CreateTilemap(3, "Dirt Marching Tilemap");

        _material = new Material(GlobalConfig.Ins.StandardMaterial);
        _material.SetFloat(Lytil.FogIntensityID, _fogScale);
        
        _baseMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _material;
        _lowerPebbleMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _material;
        _upperPebbleMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _material;
        _marchingMap.gameObject.AddComponent<TilemapRenderer>().sharedMaterial = _material;
    }

    protected override void Initialise()
    {
        _depthMap = new Datamap<int>(Holder.MapSize, 0);
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override bool CheckOverlap(Vector2 pos)
    {
        if (!Holder.SnapWorldToMap(pos, out var mapPos)) return false;
        return _depthMap.At(mapPos) > 0;
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
        while (iter.MoveNext())
            yield return iter.Current;

        yield return new PropertyHandle()
        {
            PropertyName = "Fog Intensity %",
            PropertyType = PropertyType.Decimal,
            Getter = () => _fogScale * 100f,
            Setter = (value) =>
            {
                _fogScale = (float)value / 100f;
                _material.SetFloat(Lytil.FogIntensityID, _fogScale);
            }
        };
    }

    public override float GetReferenceZ() => _baseMap.transform.position.z;
    public override string Pack() => JsonUtility.ToJson((base.Pack(), _depthMap.Pack()));

    public override void Unpack(string data)
    {
        var (basePacked, depthPacked) = JsonUtility.FromJson<(string, string)>(data);
        
        RequestInitialise();
        base.Unpack(basePacked);
        _depthMap.Unpack(depthPacked);
        
        for (var x = 0; x < _depthMap.Width; x++)
        for (var y = 0; y < _depthMap.Height; y++)
            UpdateVisualsAt(new Vector2Int(x, y));
    }

    public void ChangeAt(Vector2 rootWorldPos, bool shouldPlaceNotRemove)
    {
        if (!Holder.SnapWorldToMap(rootWorldPos, out var rootPos) ||
            (_depthMap.At(rootPos) == 0) != shouldPlaceNotRemove) return;

        var pending = new Dictionary<Vector2Int, int>
            { [rootPos] = shouldPlaceNotRemove ? 1 : 0 };

        while (pending.Count > 0)
        {
            var pos = pending.Keys.Last();
            pending.TryGetValue(pos, out var depth);
            pending.Remove(pos);

            _depthMap.At(pos) = depth;
            foreach (var neighbour in Holder.RetrievePositions(pos, Lytil.FullNeighbourOffsets))
            {
                var currentDepth = _depthMap.At(neighbour);
                var calculatedDepth = Mathf.Min(RetrieveMinNeighbourDepth(neighbour) + 1, maxDepth);
                if (currentDepth == 0 || currentDepth == calculatedDepth)
                {
                    UpdateVisualsAt(neighbour);
                    continue;
                }

                pending[neighbour] = calculatedDepth;
            }

            UpdateVisualsAt(pos);
        }
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void GeneratePhysics()
    {
        _baseMap.gameObject.AddComponent<TilemapCollider2D>();
        _baseMap.gameObject.layer = 8;
    }
    
    private void UpdateVisualsAt(Vector2Int pos)
    {
        var depth = _depthMap.At(pos);

        //layer determining
        DirtStratum? foundLayer = null;
        var lastLayerEndDepth = 0;
        if (depth != 0)
            foreach (var current in strata)
            {
                var currentLayerEndDepth = lastLayerEndDepth + current.thickness;
                if (depth <= currentLayerEndDepth)
                {
                    foundLayer = current;
                    break;
                }

                lastLayerEndDepth = currentLayerEndDepth;
            }


        // marching query
        var fullQuery = new MarchingTileQuery(new bool[Lytil.FullNeighbourOffsets.Length]);
        var halfQuery = new MarchingTileQuery(new bool[Lytil.HalfNeighbourOffsets.Length]);
        for (var i = 0; i < Lytil.FullNeighbourOffsets.Length; i++)
        {
            var n = pos + Lytil.FullNeighbourOffsets[i];
            var inBounds = Holder.IsInBounds(n);
            var present = inBounds && _depthMap.At(n) > lastLayerEndDepth;
            var check = (!inBounds && depth != 0) || present;
            fullQuery.Neighbours[i] = check;
            if (i < Lytil.HalfNeighbourOffsets.Length)
                halfQuery.Neighbours[i] = check;
        }

        //march
        var marchingSet = depth != 0 ? foundLayer?.marchingSet : outlineMarchingSet;
        var marchingTile
            = (marchingSet &&
               (marchingSet.TryGetTile(fullQuery, out var variants) ||
                marchingSet.TryGetTile(halfQuery, out variants)))
                ? variants[UnityEngine.Random.Range(0, variants.Length)]
                : null;
        _marchingMap.SetTile((Vector3Int)pos, marchingTile);


        if (foundLayer == null)
        {
            _baseMap.SetTile((Vector3Int)pos, null);
            _lowerPebbleMap.SetTile((Vector3Int)pos, null);
            _upperPebbleMap.SetTile((Vector3Int)pos, null);
            return;
        }

        var layer = foundLayer.Value;

        // base
        _baseMap.SetTile((Vector3Int)pos, layer.baseTile);

        //pebbles
        Random.InitState(pos.x * 100 + pos.y);
        var rndLower = Random.Range(0, 10000);
        var shouldPlaceLower = rndLower <= layer.lowerPebbleDensity * 10000f;
        var lowerPebbles = layer.lowerPebbles;
        var lowerPebble = (shouldPlaceLower && lowerPebbles?.Length > 0)
            ? lowerPebbles[rndLower % lowerPebbles.Length]
            : null;
        _lowerPebbleMap.SetTile((Vector3Int)pos, lowerPebble);


        Random.InitState(rndLower);
        var rndUpper = Random.Range(0, 10000);
        var shouldPlaceUpper = rndUpper <= layer.upperPebbleDensity * 10000f;
        var upperPebbles = layer.upperPebbles;
        var upperPebble = (shouldPlaceUpper && upperPebbles?.Length > 0)
            ? upperPebbles[rndUpper % upperPebbles.Length]
            : null;
        _upperPebbleMap.SetTile((Vector3Int)pos, upperPebble);
    }

    private int RetrieveMinNeighbourDepth(Vector2Int pos)
    {
        var minDepth = maxDepth;
        foreach (var neighbour in Holder.RetrievePositions(pos, Lytil.FullNeighbourOffsets))
        {
            var depth = _depthMap.At(neighbour);
            if (depth < minDepth) minDepth = depth;
        }

        return minDepth;
    }
}

}