using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorController : MonoBehaviour
{
    [SerializeField] private LevelHolder levelHolder;
    [SerializeField] private Camera controlCamera;

    private Vector3 mapOrigin;

    private void Start()
    {
        mapOrigin = levelHolder.GetOrigin();
    }

    void Update()
    {
        var constructing = Input.GetMouseButton(0);
        var erasing = Input.GetMouseButton(1);
        
        if (!constructing && !erasing)
            return;
        
        var mousePos = Input.mousePosition;
        var ray = controlCamera.ScreenPointToRay(mousePos);

        var t = (mapOrigin.z - ray.origin.z) / ray.direction.z;
        var mapPos = ray.origin + ray.direction * t - mapOrigin;

        var blockType = BlockType.None;
        if (constructing)
            blockType = BlockType.Dirt;
        
        levelHolder.ChangeBlockAt(mapPos, blockType);
    }
}
