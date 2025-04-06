using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class SpriteOverlap : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SpriteRenderer spriteRenderer;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(spriteRenderer);
        var overlapAccessor = new EntityOverlapAccessor(pos => RockUtil.IsInRendererBounds(pos, spriteRenderer));
        Entity.AddAccessor("overlap", overlapAccessor);
    }

    public override void Initialise() { }
    public override void Activate() {}

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "sprite-overlap";
    public override IEnumerator<PropertyHandle> GetProperties() {yield break;}
    public override void Replicate(JSONNode data) {}
    public override JSONNode ExtractData() => new JSONObject();
    
    
}

}