
using System;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectManipulator : ManipulatorBase, IPlaceRemoveHandler
{
    [SerializeField] private GameObject prefabToUse;
    
    private EditorController _controller;
    private Transform _manipulatedTransform;
    
    private void Awake()
    {
        Assert.IsNotNull(prefabToUse);
        _manipulatedTransform = Instantiate(prefabToUse, Target, false).transform;
    }

    private void Start()
    {
        holder.RegisterAt(this, 3);
    }

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
}