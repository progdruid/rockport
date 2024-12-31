using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class PropEntity : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private MonoBehaviour parametrisedScript;
    
    private Material _material;
    private float _fogScale = 0f;
    
    private PhysicalEntityTrait _physicalTrait;
    private IPropertyHolder _parametrisedPropertyHolder;
    private IPackable _parametrisedPackable;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        Assert.IsNotNull(parametrisedScript);
        _parametrisedPropertyHolder = parametrisedScript as IPropertyHolder;
        _parametrisedPackable = parametrisedScript as IPackable;
        
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

        if (_parametrisedPropertyHolder == null) yield break;
        var pIter = _parametrisedPropertyHolder.GetProperties();
        while (pIter.MoveNext())
            yield return pIter.Current;
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
        Holder.SnapWorldToMap(Target.position, out var map);
        object data = _parametrisedPackable != null
            ? (_physicalTrait.Pack(), _parametrisedPackable.Pack(), map)
            : (_physicalTrait.Pack(), map);
        return JsonUtility.ToJson(data);
    }

    public override void Unpack(string data)
    {
        string physicalPacked;
        string parametrisedPacked = null;
        Vector2Int mapPos;
        if (_parametrisedPackable != null)
            (physicalPacked, parametrisedPacked, mapPos) = JsonUtility.FromJson<(string, string, Vector2Int)>(data);
        else
            (physicalPacked, mapPos) = JsonUtility.FromJson<(string, Vector2Int)>(data);
        
        RequestInitialise();
        _physicalTrait.Unpack(physicalPacked);
        _parametrisedPackable?.Unpack(parametrisedPacked);
        var snappedWorldPos = Holder.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Holder.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Holder.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }

    public override void Activate() => _physicalTrait.RequestGeneratePhysics();
}

}