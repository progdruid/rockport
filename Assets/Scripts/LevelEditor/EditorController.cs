using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorController : MonoBehaviour
{
    [SerializeField] private LevelHolder levelHolder;
    [SerializeField] private Camera controlCamera;

    private Vector3 _mapOrigin;
    private Datamap _typeDatamap;
    
    private void Start()
    {
        _mapOrigin = levelHolder.GetOrigin();
        _typeDatamap = levelHolder.ObtainDatamap<BlockType>("Type");
    }

    void Update()
    {
        var constructing = Input.GetMouseButton(0);
        var erasing = Input.GetMouseButton(1);
        
        if (!constructing && !erasing)
            return;
        
        var mousePos = Input.mousePosition;
        var ray = controlCamera.ScreenPointToRay(mousePos);

        var t = (_mapOrigin.z - ray.origin.z) / ray.direction.z;
        var mapPos = levelHolder.ConvertWorldToMapPos(ray.origin + ray.direction * t - _mapOrigin);
        
        var blockType = BlockType.None;
        if (constructing)
            blockType = BlockType.Dirt;

        if (_typeDatamap.At<BlockType>(mapPos) == blockType)
            return;
        
        _typeDatamap.At(mapPos) = blockType;
        _typeDatamap.NotifyModified(mapPos);
    }
}
