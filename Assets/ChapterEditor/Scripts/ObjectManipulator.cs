using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace ChapterEditor
{

public class ObjectManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SerializableMap<string, GameObject> prefabs = new();

    private EditorController _controller;
    private Transform _manipulatedTransform;
    private string _usedPrefabName = "";

    private Material _material;
    private float _fogScale = 0f;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        _material = new Material(GlobalConfig.Ins.StandardMaterial);
        _material.SetFloat(Lytil.FogIntensityID, _fogScale);
    }

    protected override void Initialise() { }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;

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
    public override void SubscribeInput(EditorController controller)
    {
        controller.SetPlaceRemoveHandler(this);
        _controller = controller;
    }
    public override void UnsubscribeInput()
    {
        _controller.UnsetPlaceRemoveHandler();
        _controller = null;
    }
    public override string Pack()
    {
        Holder.SnapWorldToMap(_manipulatedTransform.position, out var map);
        return JsonUtility.ToJson((base.Pack(), _usedPrefabName, map));
    }

    public override void Unpack(string data)
    {
        var (basePacked, usedName, mapPos) = JsonUtility.FromJson<(string, string, Vector2Int)>(data);
        
        RequestInitialise();
        base.Unpack(basePacked);
        UpdateObjectToName(usedName);
        _manipulatedTransform.localPosition = Holder.ConvertMapToWorld(mapPos);
    }

    public override void KillDrop()
    {
        _manipulatedTransform.SetParent(Target.parent, true);
        var thisObject = gameObject;
        base.KillDrop();
        Destroy(thisObject);
    }

    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Holder.SnapWorldToMap(worldPos, out var mapPos) || !_manipulatedTransform) return;
        var snappedWorldPos = Holder.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void GeneratePhysics()
    {
        TogglePhysicsInObject(_manipulatedTransform, true);
    }
    
    private void UpdateObjectToName(string prefabName)
    {
        if (prefabName == _usedPrefabName) return;

        if (_manipulatedTransform)
            Destroy(_manipulatedTransform.gameObject);

        _manipulatedTransform = null;
        _usedPrefabName = "";
        if (prefabs.TryGetValue(prefabName, out var prefab))
        {
            _manipulatedTransform = Instantiate(prefab, Target, false).transform;
            TogglePhysicsInObject(_manipulatedTransform, false);
            
            var renderers = _manipulatedTransform.GetComponents<Renderer>()
                .Concat(_manipulatedTransform.GetComponentsInChildren<Renderer>(true));
            foreach (var rend in renderers) rend.sharedMaterial = _material;

            _usedPrefabName = prefabName;
        }

        InvokePropertiesChangeEvent();
    }

    private void TogglePhysicsInObject(Transform obj, bool value)
    {
        var bodies = obj.GetComponents<Rigidbody2D>().Concat(obj.GetComponentsInChildren<Rigidbody2D>(true));
        var colliders = obj.GetComponents<Collider2D>().Concat(obj.GetComponentsInChildren<Collider2D>(true));
        
        foreach (var col in colliders) col.enabled = value;
        foreach (var body in bodies)
        {
            if (value) body.WakeUp();
            else body.Sleep();
        }
    }
}

}