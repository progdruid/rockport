using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class Decor : EntityComponent
{
    private static readonly int UVRect = Shader.PropertyToID("_UVRect");
    private static readonly int FlipX = Shader.PropertyToID("_FlipX");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private bool pickRandom = false;
    
    private int _spriteIndex = 0;
    private bool _flip = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(sprites);
        Assert.IsTrue(sprites.Length > 0);
        Assert.IsTrue(sprites[0]);
        
        if (pickRandom) _spriteIndex = Random.Range(0, sprites.Length);
        UpdateSpriteToIndex();
    }
    public override void Initialise() {}
    public override void Activate() {}

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "decor";

    public override IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Sprite",
            PropertyType = PropertyType.Integer,
            Getter = () => _spriteIndex,
            Setter = (value) =>
            {
                var index = ((int)value).ClampBottom(0).ClampTop(sprites.Length - 1);
                _spriteIndex = index;
                UpdateSpriteToIndex();
            }
        };
        yield return new PropertyHandle()
        {
            PropertyName = "Flip",
            PropertyType = PropertyType.Text,
            Getter = () => _flip ? "true" : "false",
            Setter = (object input) =>
            {
                _flip = (string)input == "true";
                UpdateFlipX();
            }
        };
    }

    public override void Replicate(JSONNode data)
    {
        var index = data["spriteIndex"].AsInt;
        _spriteIndex = index.ClampBottom(0).ClampTop(sprites.Length - 1);
        
        _flip = data["flip"].AsBool;
        UpdateSpriteToIndex();
        UpdateFlipX();
        InvokePropertiesChangeEvent();
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject
        {
            ["spriteIndex"] = _spriteIndex,
            ["flip"] = _flip
        };
        return json;
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateSpriteToIndex()
    {
        spriteRenderer.sprite = sprites[_spriteIndex];

        if (!spriteRenderer.material.HasProperty(UVRect)) return;
        
        var pixelRect = spriteRenderer.sprite.textureRect;
        var textureSize = new Vector2(
            spriteRenderer.sprite.texture.width,
            spriteRenderer.sprite.texture.height
        );
        var uvValues = new Vector4(
            pixelRect.x / textureSize.x,
            pixelRect.y / textureSize.y,
            pixelRect.width / textureSize.x,
            pixelRect.height / textureSize.y
        );
        spriteRenderer.material.SetVector(UVRect, uvValues);
    }

    private void UpdateFlipX()
    {
        if (spriteRenderer.material.HasProperty(FlipX))
            spriteRenderer.material.SetFloat(FlipX, _flip ? 1f : 0f);
        else 
            spriteRenderer.flipX = _flip;
    }
}

}