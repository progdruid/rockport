using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assert = UnityEngine.Assertions.Assert;

namespace Map
{

public class SpikeLayerEntity : EntityComponent, ITileLayerAccessor
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Spikes")] 
    [SerializeField] private SerializableMap<string, TileBase> tiles;
    
    private TileBase[] _tileRegister;
    private readonly Dictionary<string, byte> _nameToIndex = new ();
    
    private string _selectedTileName = "None";
    private TileBase _selectedTile = null;
    private byte _selectedIndex = 0;

    private Datamap<byte> _placed;
    
    private Tilemap _tilemap;
    private UniversalTrigger _trigger = null;

    MapEntity IEntityAccessor.Entity { get; set; }
    string IEntityAccessor.AccessorName { get; set; }

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        var overlapAccessor = new EntityOverlapAccessor(pos => 
            Space.SnapWorldToMap(pos, out var mapPos) 
            && _placed.At(mapPos) != 0);

        Entity.AddAccessor("tile-layer", this);
        Entity.AddAccessor("overlap", overlapAccessor);
        
        _tileRegister = new TileBase[tiles.Count+1];
        _tileRegister[0] = null;
        byte index = 1;
        foreach (var (tileName, tile) in tiles)
        {
            Assert.IsNotNull(tile);
            _tileRegister[index] = tile;
            _nameToIndex[tileName] = index;
            index++;
        }

        _tilemap = RockUtil.CreateTilemap(Target, 0, "Spike Map");
        _tilemap.gameObject.AddComponent<TilemapRenderer>();
        _tilemap.gameObject.layer = 7;
    }

    public override void Initialise()
    {
        _placed = new Datamap<byte>(Space.MapSize, 0);
    }
    
    public override void Activate()
    {
        _tilemap.gameObject.AddComponent<TilemapCollider2D>();
        _tilemap.gameObject.layer = Target.gameObject.layer;
        _trigger = _tilemap.gameObject.AddComponent<UniversalTrigger>();
        _trigger.EnterEvent += HandleTriggerEnter;
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "spikes";
    public override IEnumerator<PropertyHandle> GetProperties()
    {
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
                _selectedIndex = _nameToIndex[tileName];
            }
        };
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject {
            ["placed"] = _placed.ExtractData()
        };
        return json;
    }

    public override void Replicate(JSONNode data)
    {
        var placedPacked = data["placed"];

        _placed.Replicate(placedPacked);

        for (var x = 0; x < _placed.Width; x++)
        for (var y = 0; y < _placed.Height; y++)
        {
            var pos = new Vector2Int(x, y);
            var index = _placed.At(pos);
            if (index == 0) continue;

            _tilemap.SetTile((Vector3Int)pos, _tileRegister[index]);
        }
    }
    
    public void ChangeAtWorldPos(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var rootPos)) return;
        ChangeAtMapPos(rootPos, shouldPlaceNotRemove);
    }

    public void ChangeAtMapPos(Vector2Int mapPos, bool shouldPlaceNotRemove)
    {
        if (!_selectedTile  
            || (!shouldPlaceNotRemove && _placed.At(mapPos) == 0)
            || (shouldPlaceNotRemove && _placed.At(mapPos) == _selectedIndex)) 
            return;
        
        if (shouldPlaceNotRemove)
        {
            _placed.At(mapPos) = _selectedIndex;
            _tilemap.SetTile((Vector3Int)mapPos, _selectedTile);
        }
        else
        {
            _placed.At(mapPos) = 0;
            _tilemap.SetTile((Vector3Int)mapPos, null);
        }
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleTriggerEnter (Collider2D col, TriggeredType type)
    {
        if (type != TriggeredType.Player)
            return;

        GameSystems.Ins.PlayerManager.KillPlayer();
    }
}

}
