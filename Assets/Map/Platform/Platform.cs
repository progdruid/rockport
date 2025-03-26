using System.Collections.Generic;
using System.Linq;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

public class Platform : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Platform")]
    [SerializeField] private float colliderHeight;
    [SerializeField] private float outlineOffset = 0.0625f;
    [SerializeField] private Sprite[] sprites;

    private readonly List<SpriteRenderer> _spriteRenderers = new ();
    private BoxCollider2D _platformCollider = null;

    private int _platformWidth = 2;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        Assert.IsNotNull(sprites);
        foreach (var sprite in sprites)
            Assert.IsNotNull(sprite);
        
        GenerateSprites();
    }
    
    public override void Activate()
    {
        _platformCollider = Target.gameObject.AddComponent<BoxCollider2D>();
        _platformCollider.size = new Vector2(_platformWidth * 0.5f, colliderHeight);
        _platformCollider.offset = new Vector2(_platformCollider.size.x * 0.5f, 0.5f - _platformCollider.size.y * 0.5f);
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;

        yield return new PropertyHandle()
        {
            PropertyName = "Width",
            PropertyType = PropertyType.Integer,
            Getter = () => _platformWidth,
            Setter = (object input) =>
            {
                _platformWidth = ((int)input).ClampBottom(2);
                GenerateSprites();
            }
        };
    }

    public override bool CheckOverlap(Vector2 pos)
    {
        if (!Space.SnapWorldToMap(pos, out var checkPos))
            return false;
        
        var inbounds = Space.SnapWorldToMap(Target.position.To2(), out var platformPos);
        Assert.IsTrue(inbounds);
        
        return checkPos.y == platformPos.y 
               && checkPos.x >= platformPos.x 
               && checkPos.x < platformPos.x + _platformWidth;
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject();
        var inBounds = Space.SnapWorldToMap(Target.position, out var mapPos);
        Assert.IsTrue(inBounds);
        json["pos"] = mapPos.ToJson();
        json["width"] = _platformWidth;
        return json;
    }

    public override void Replicate(JSONNode data)
    {
        var world = Space.ConvertMapToWorld(data["pos"].ReadVector2Int());
        Target.SetWorldXY(world);
        
        _platformWidth = data["width"].AsInt;
        GenerateSprites();
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void GenerateSprites()
    {
        foreach (var spriteObject in _spriteRenderers) 
            Destroy(spriteObject);
        _spriteRenderers.Clear();
        
        var rand = new System.Random();

        for (var i = 0; i < _platformWidth; i++)
        {
            var piece = new GameObject("Platform_" + i);
            piece.transform.SetParent(Target, false);
            piece.transform.SetLocalXY(i * 0.5f + 0.25f, 0.25f+outlineOffset);
            
            var spriteRenderer = piece.AddComponent<SpriteRenderer>();
            _spriteRenderers.Add(spriteRenderer);
        }

        _spriteRenderers.First().sprite = sprites.First();

        for (var i = 1; i < _platformWidth - 1; i++)
        {
            var seed = rand.Next();
            _spriteRenderers[i].sprite = sprites[seed % (sprites.Length - 2) + 1];
            rand = new System.Random(seed);
        }
        
        _spriteRenderers.Last().sprite = sprites.Last();
    }
}
