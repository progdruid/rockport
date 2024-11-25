
using System;
using System.Collections.Generic;
using System.Linq;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject prefabToUse;
    
    private EditorController _controller;
    private Transform _manipulatedTransform;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(prefabToUse);
        _manipulatedTransform = Instantiate(prefabToUse, Target, false).transform;
    }

    private void Start()
    {
        holder.RegisterAt(this, 3);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!holder.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = holder.ConvertMapToWorld(mapPos);
        _manipulatedTransform.localPosition = snappedWorldPos;
    }

    public float GetZForInteraction() => Target.position.z;
    
    public override void UnsubscribeInput()
    {
        _controller.UnsetPropertyHolder();
        _controller.UnsetPlaceRemoveHandler();
        _controller = null;
    }

    public override void SubscribeInput(EditorController controller)
    {
        controller.SetPlaceRemoveHandler(this);
        controller.SetPropertyHolder(this);
        _controller = controller;
    }
    
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        var iter = base.GetProperties();
        while(iter.MoveNext())
            yield return iter.Current;

        foreach (var propertyHolder in _manipulatedTransform.GetComponents<Component>().OfType<IPropertyHolder>())
        {
            var pIter = propertyHolder.GetProperties();
            while (pIter.MoveNext())
                yield return pIter.Current;
        }
    }
}