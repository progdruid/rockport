using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace ChapterEditor
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
        _material.SetFloat(Lytil.FogIntensityID, _fogScale);
        
        foreach (var rend in GetComponentsInChildren<Renderer>(true)) 
            rend.sharedMaterial = _material;

        _physicalTrait = new PhysicalEntityTrait();
        _physicalTrait.PropertiesChangeEvent += InvokePropertiesChangeEvent;
        _physicalTrait.AddObject(gameObject);
    }

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
                _material.SetFloat(Lytil.FogIntensityID, _fogScale);
            }
        };
    }

    public override float GetReferenceZ() => Target.position.z;
    public override bool CheckOverlap(Vector2 pos)
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        return renderers.Any(spriteRenderer =>
            Lytil.IsInBounds(pos, spriteRenderer.bounds.min, spriteRenderer.bounds.max));
    }

    public override string Pack()
    {
        Space.SnapWorldToMap(Target.position, out var map);
        return JsonUtility.ToJson((_physicalTrait.Pack(), map));
    }

    public override void Unpack(string data)
    {
        var (physicalPacked, mapPos) = JsonUtility.FromJson<(string, Vector2Int)>(data);
        
        RequestInitialise();
        _physicalTrait.Unpack(physicalPacked);
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }

    public override void Activate() => _physicalTrait.RequestGeneratePhysics();
}

}