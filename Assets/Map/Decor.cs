using System;
using System.Collections.Generic;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class Decor : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    
    private bool _flip = false;
    private int _spriteIndex = 0;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(sprites);
        Assert.IsTrue(sprites.Length > 0);
        Assert.IsTrue(sprites[0]);
        
        spriteRenderer.sprite = sprites[_spriteIndex];
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
                spriteRenderer.sprite = sprites[_spriteIndex];
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
                spriteRenderer.flipX = _flip;
            }
        };
    }

    public override void Replicate(JSONNode data)
    {
        var index = data["spriteIndex"].AsInt;
        _spriteIndex = index.ClampBottom(0).ClampTop(sprites.Length - 1);
        
        _flip = data["flip"].AsBool;
        spriteRenderer.sprite = sprites[_spriteIndex];
        spriteRenderer.flipX = _flip;
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
    
}

}