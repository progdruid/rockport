using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Map
{

public class PropEntity : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private Material _material;
    private float _fogScale = 0f;
    
    private PhysicalEntityTrait _physicalTrait;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        _material = new Material(GlobalConfig.Ins.StandardMaterial);
        _material.SetFloat(RockUtil.FogIntensityID, _fogScale);
        
        foreach (var rend in GetComponentsInChildren<Renderer>(true)) 
            rend.sharedMaterial = _material;

        _physicalTrait = new PhysicalEntityTrait();
        _physicalTrait.PropertiesChangeEvent += InvokePropertiesChangeEvent;
        _physicalTrait.AddObject(gameObject);
    }
    
    public override void Activate() => _physicalTrait.RequestGeneratePhysics();

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
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
                _material.SetFloat(RockUtil.FogIntensityID, _fogScale);
            }
        };
    }

    public override float GetReferenceZ() => Target.position.z;
    public override bool CheckOverlap(Vector2 pos)
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        return renderers.Any(spriteRenderer => RockUtil.IsInRendererBounds(pos, spriteRenderer));
    }

    public override JSONObject ExtractData()
    {
        Space.SnapWorldToMap(Target.position, out var map);
        
        var json = new JSONObject();
        json["physicalTrait"] = _physicalTrait.ExtractData();
        json["mapPos"] = map.ToJson();
        return json;
    }

    public override void Replicate(JSONObject data)
    {
        var physicalPacked = data["physicalTrait"].AsObject;
        var mapPos = data["mapPos"].ReadVector2Int();

        RequestInitialise();
        _physicalTrait.Replicate(physicalPacked);
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }
}

}