using System;
using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public abstract class ManipulatorBase : MonoBehaviour, IPropertyHolder
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private string manipulatorName;
    [SerializeField] private Transform target;
    [SerializeField] protected LevelSpaceHolder holder;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected virtual void Awake()
    {
        Assert.IsNotNull(target);
    }
    
    //abstract functionality////////////////////////////////////////////////////////////////////////////////////////////
    public abstract void SubscribeInput(EditorController controller);
    public abstract void UnsubscribeInput();
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event Action PropertiesChangeEvent;

    public string ManipulatorName
    {
        get => manipulatorName;
        protected set => manipulatorName = value;
    }

    public Transform Target => target;
    public void InjectHolder (LevelSpaceHolder injected) => holder = injected;


    public virtual IEnumerator<PropertyHandle> GetProperties()
    {
        var handle = new PropertyHandle()
        {
            PropertyName = "Name",
            PropertyType = PropertyType.Text,
            Getter = () => manipulatorName,
            Setter = (object input) => manipulatorName = (string)input
        };
        yield return handle;
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected void InvokePropertiesChangeEvent() => PropertiesChangeEvent?.Invoke();
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
}
