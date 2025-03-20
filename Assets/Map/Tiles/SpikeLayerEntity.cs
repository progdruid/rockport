using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assert = UnityEngine.Assertions.Assert;

namespace Map
{

public class SpikeLayerEntity : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Spikes")] 
    [SerializeField] private SerializableMap<string, TileBase> tiles;

    private TileBase[] _tileRegister;
    private Dictionary<string, byte> _nameToIndex = new ();
    
    private Tilemap _tilemap;
    private Datamap<byte> _placed;

    private string _selectedTileName = "None";
    private TileBase _selectedTile = null;
    
    private UniversalTrigger _trigger;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();

        _tileRegister = new TileBase[tiles.Count];
        int i 
        foreach (var (tileName, tile) in tiles)
        {
            Assert.IsNotNull(tile);
            
        }

        
        
        _tilemap = RockUtil.CreateTilemap(Target, 0, "Spike Map");
        _tilemap.gameObject.AddComponent<TilemapRenderer>();
        _tilemap.gameObject.AddComponent<TilemapCollider2D>();
        _trigger = _tilemap.gameObject.AddComponent<UniversalTrigger>();
    }
    
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;
        
        yield return new PropertyHandle()
        {
            PropertyName = "Selected Tile",
            PropertyType = PropertyType.Text,
            Getter = () => _selectedTileName,
            Setter = (object input) =>
            {
                Assert.IsTrue(input is string);
                var tileName = (string)input;
                if (!tiles.TryGetValue(tileName, out var tile)) 
                    return;
                _selectedTileName = tileName;
                _selectedTile = tile;
            }
        };
    }

    public override bool CheckOverlap(Vector2 pos) 
        => Space.SnapWorldToMap(pos, out var mapPos) && _placed.At(mapPos);

    public override string Pack()
    {
        var data = _placed.Pack();
        return JsonUtility.ToJson(data);
    }

    public override void Unpack(string data)
    {
        RequestInitialise();
        _placed.Unpack(data);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!_selectedTile 
            || !Space.SnapWorldToMap(worldPos, out var pos) 
            || shouldPlaceNotRemove == _placed.At(pos)) 
            return;
        
        _placed.At(pos) = shouldPlaceNotRemove;
        _tilemap.SetTile((Vector3Int)pos, _selectedTile);
    }

    public override void Activate()
    {
        base.Activate();

        if (_generateCollider)
            _tilemap.gameObject.AddComponent<TilemapCollider2D>();
    }
}

}
