using System.Collections.Generic;
using System.Linq;
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


    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Initialise() { }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while (iter.MoveNext())
            yield return iter.Current;

        var usedPrefabNameHandle = new PropertyHandle()
        {
            PropertyName = "Used Prefab",
            PropertyType = PropertyType.Text,
            Getter = () => _usedPrefabName,
            Setter = (object val) => UpdateObjectToName(val.ToString())
        };
        yield return usedPrefabNameHandle;

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
        Registry.SnapWorldToMap(_manipulatedTransform.position, out var map);
        return JsonUtility.ToJson((_usedPrefabName, map));
    }

    public override void Unpack(string data)
    {
        RequestInitialise();
        var (usedName, mapPos) = JsonUtility.FromJson<(string, Vector2Int)>(data);
        UpdateObjectToName(usedName);
        _manipulatedTransform.localPosition = Registry.ConvertMapToWorld(mapPos);
    }
    public override void KillDrop()
    {
        _manipulatedTransform.SetParent(Target.parent, true);
        base.KillDrop();
    }

    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Registry.SnapWorldToMap(worldPos, out var mapPos) || !_manipulatedTransform) return;
        var snappedWorldPos = Registry.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
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
            _usedPrefabName = prefabName;
        }

        InvokePropertiesChangeEvent();
    }
}

}