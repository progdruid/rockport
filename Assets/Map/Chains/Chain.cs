using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class Chain : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] chainVariants;

    private int _variantIndex = 0;
    private int _segments = 1;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(chainVariants);
        Assert.IsTrue(chainVariants.Length > 0);
        Assert.IsTrue(chainVariants[0]);

        spriteRenderer.sprite = chainVariants[_variantIndex];
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        UpdateSize();
    }

    public override void Initialise() {}
    public override void Activate() {}

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "chain";

    public override IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Variant",
            PropertyType = PropertyType.Integer,
            Getter = () => _variantIndex,
            Setter = (value) =>
            {
                var index = ((int)value).ClampBottom(0).ClampTop(chainVariants.Length - 1);
                _variantIndex = index;
                spriteRenderer.sprite = chainVariants[_variantIndex];
                UpdateSize();
            }
        };

        yield return new PropertyHandle()
        {
            PropertyName = "Number of Segments",
            PropertyType = PropertyType.Integer,
            Getter = () => _segments,
            Setter = (value) =>
            {
                _segments = ((int)value).ClampBottom(1);
                UpdateSize();
            }
        };
    }

    public override void Replicate(JSONNode data)
    {
        var variant = data["variant"].AsInt;
        _variantIndex = variant.ClampBottom(0).ClampTop(chainVariants.Length - 1);
        _segments = data["segments"].AsInt.ClampBottom(1);
        
        spriteRenderer.sprite = chainVariants[_variantIndex];
        UpdateSize();
        InvokePropertiesChangeEvent();
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject
        {
            ["variant"] = _variantIndex,
            ["segments"] = _segments
        };
        return json;
    }

    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateSize()
    {
        var unitSize = chainVariants[_variantIndex].bounds.size;
        spriteRenderer.size = new Vector2(unitSize.x, _segments * unitSize.y);
    }
}

}