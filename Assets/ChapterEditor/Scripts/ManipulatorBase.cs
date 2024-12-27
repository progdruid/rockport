using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace ChapterEditor
{

public abstract class ManipulatorBase : MonoBehaviour, IPropertyHolder, IPackable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private string manipulatorName;
    [SerializeField] private Transform target;

    private bool _initialised = false;
    private bool _generatePhysics;

    protected MapSpaceHolder Holder { get; private set; }

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected virtual void Awake() => Assert.IsNotNull(target);
    protected void Start() => RequestInitialise();
    protected abstract void Initialise();

    protected void RequestInitialise()
    {
        if (_initialised) return;
        _initialised = true;
        Initialise();
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event Action PropertiesChangeEvent;

    public string ManipulatorName => manipulatorName;
    public Transform Target => target;

    public void InjectHolder(MapSpaceHolder injected) => Holder = injected;

    
    public virtual IEnumerator<PropertyHandle> GetProperties()
    {
        var titleHandle = new PropertyHandle()
        {
            PropertyName = "Title",
            PropertyType = PropertyType.Text,
            Getter = () => manipulatorName,
            Setter = (object input) => manipulatorName = (string)input
        };
        yield return titleHandle;
        
        var physicsHandle = new PropertyHandle()
        {
            PropertyName = "Generate Physics",
            PropertyType = PropertyType.Text,
            Getter = () => _generatePhysics ? "true" : "false",
            Setter = (object input) => _generatePhysics = (string)input == "true"
        };
        yield return physicsHandle;
        
    }

    public abstract float GetReferenceZ();
    public virtual bool CheckOverlap (Vector2 pos) => false;
    
    public abstract void SubscribeInput(EditorController controller);
    public abstract void UnsubscribeInput();
    public virtual string Pack() => _generatePhysics ? "true" : "false";
    public virtual void Unpack(string data)
    {
        _generatePhysics = data == "true";
        InvokePropertiesChangeEvent();
    }

    public virtual void KillDrop()
    {
        if (_generatePhysics)
            GeneratePhysics();
        Destroy(this);
    }

    public void KillClean()
    {
        var targetObject = target.gameObject;
        Destroy(this);
        Destroy(targetObject);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected abstract void GeneratePhysics();
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

}