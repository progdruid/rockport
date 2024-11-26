
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
    private void Start()
    {
        holder.RegisterAt(this, 3);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!holder.SnapWorldToMap(worldPos, out var mapPos) || !_manipulatedTransform) return;
        var snappedWorldPos = holder.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }

    public float GetZForInteraction() => Target.position.z;
    
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
            Setter = (object val) =>
            {
                _usedPrefabName = val.ToString();
                UpdatePrefab(_usedPrefabName);
            }
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

    private void UpdatePrefab(string prefabName)
    {
        if (_manipulatedTransform)
            Destroy(_manipulatedTransform.gameObject);
        
        _manipulatedTransform = prefabs.TryGetValue(prefabName, out var prefab)
            ? Instantiate(prefab, Target, false).transform : null;

        InvokePropertiesChangeEvent();
    }
}