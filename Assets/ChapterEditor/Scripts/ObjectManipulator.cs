using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace ChapterEditor
{

public class ObjectManipulator : ManipulatorBase
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SerializableMap<string, GameObject> prefabs = new();

    private Transform _manipulatedTransform;
    private string _usedPrefabName = "";

    private Material _material;
    private float _fogScale = 0f;
    
    private PhysicalManipulatorTrait _physicalTrait;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        _material = new Material(GlobalConfig.Ins.StandardMaterial);
        _material.SetFloat(Lytil.FogIntensityID, _fogScale);

        _physicalTrait = new PhysicalManipulatorTrait();
        _physicalTrait.PropertiesChangeEvent += InvokePropertiesChangeEvent;
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
            PropertyName = "Object",
            PropertyType = PropertyType.Text,
            Getter = () => _usedPrefabName,
            Setter = (object val) => UpdateObjectToName(val.ToString())
        };

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

        if (!_manipulatedTransform) yield break;
        foreach (var propertyHolder in _manipulatedTransform.GetComponents<Component>().OfType<IPropertyHolder>())
        {
            var pIter = propertyHolder.GetProperties();
            while (pIter.MoveNext())
                yield return pIter.Current;
        }
    }
    public override float GetReferenceZ() => Target.position.z;
    public override bool CheckOverlap(Vector2 pos)
    {
        if (!_manipulatedTransform) return false;
        var renderers = _manipulatedTransform.GetComponentsInChildren<SpriteRenderer>();
        return renderers.Any(spriteRenderer =>
            Lytil.IsInBounds(pos, spriteRenderer.bounds.min, spriteRenderer.bounds.max));
    }

    public override string Pack()
    {
        Holder.SnapWorldToMap(_manipulatedTransform.position, out var map);
        return JsonUtility.ToJson((_physicalTrait.Pack(), _usedPrefabName, map));
    }

    public override void Unpack(string data)
    {
        var (physicalPacked, usedName, mapPos) = JsonUtility.FromJson<(string, string, Vector2Int)>(data);
        
        RequestInitialise();
        _physicalTrait.Unpack(physicalPacked);
        UpdateObjectToName(usedName);
        _manipulatedTransform.localPosition = Holder.ConvertMapToWorld(mapPos);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Holder.SnapWorldToMap(worldPos, out var mapPos) || !_manipulatedTransform) return;
        var snappedWorldPos = Holder.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void HandleRelease() => _physicalTrait.RequestGeneratePhysics();

    private void UpdateObjectToName(string prefabName)
    {
        if (prefabName == _usedPrefabName) return;

        if (_manipulatedTransform)
        {
            _physicalTrait.RemoveObject(_manipulatedTransform.gameObject);
            Destroy(_manipulatedTransform.gameObject);
        }

        _manipulatedTransform = null;
        _usedPrefabName = "";
        if (prefabs.TryGetValue(prefabName, out var prefab))
        {
            _manipulatedTransform = Instantiate(prefab, Target, false).transform;
            _physicalTrait.AddObject(_manipulatedTransform.gameObject);

            foreach (var rend in _manipulatedTransform.GetComponentsInChildren<Renderer>(true)) 
                rend.sharedMaterial = _material;

            _usedPrefabName = prefabName;
        }

        InvokePropertiesChangeEvent();
    }
}

}