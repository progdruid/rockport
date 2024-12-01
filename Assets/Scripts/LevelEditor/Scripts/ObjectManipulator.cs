
using System;
using System.Collections.Generic;
using System.Linq;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;

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
    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!holder.SnapWorldToMap(worldPos, out var mapPos) || !_manipulatedTransform) return;
        var snappedWorldPos = holder.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }

    public override float GetReferenceZ() => Target.position.z;
    public override string SerializeData()
    {
        var pos = _manipulatedTransform.localPosition;
        return $"{_usedPrefabName} {pos.x} {pos.y}";
    }

    public override void DeserializeData(string data)
    {
        RequestInitialise();
        var split = data.Split(' ');
        UpdateObjectToName(split[0]);
        _manipulatedTransform.localPosition = new Vector3(float.Parse(split[1]), float.Parse(split[2]), 0f);
    }

    public override void UnsubscribeInput()
    {
        _controller.UnsetPlaceRemoveHandler();
        _controller = null;
    }

    public override void SubscribeInput(EditorController controller)
    {
        controller.SetPlaceRemoveHandler(this);
        _controller = controller;
    }
    
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while(iter.MoveNext())
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