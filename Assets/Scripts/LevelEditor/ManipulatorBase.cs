using System;
using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public abstract class ManipulatorBase : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] protected LevelSpaceHolder holder;
    
    public Transform Target => target;
    
    public abstract void SubscribeInput(EditorController controller);
    public abstract void UnsubscribeInput();

    public void InjectHolder (LevelSpaceHolder injected) => holder = injected;
    
    protected Tilemap CreateTilemap(int offset, string mapName)
    {
        var go = new GameObject(mapName);
        go.transform.SetParent(target, false);
        go.transform.localPosition = Vector3.back * 0.01f * offset;
        var map = go.AddComponent<Tilemap>();
        map.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        map.orientation = Tilemap.Orientation.XY;
        return map;
    }

    private void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(holder);
    }
}
