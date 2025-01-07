using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public abstract class MapEntity : MonoBehaviour, IPropertyHolder, IPackable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private string title;
    [SerializeField] private Transform target;
    
    protected MapSpace Space { get; private set; }
    private readonly IntReference _layerHandle = new();
    
    private bool _initialised = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected virtual void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(title);
        Assert.IsTrue(title.Length > 0);
    }
    protected void Start() => RequestInitialise();
    protected virtual void Initialise() { }

    protected void RequestInitialise()
    {
        if (_initialised) return;
        _initialised = true;
        Initialise();
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string Title => title;
    public Transform Target => target;
    public int Layer => _layerHandle.Value;
    public SignalEmitter Emitter => emitter;
    public SignalListener[] Listeners => listeners;
    
    public IntReference InjectMap(MapSpace injected)
    {
        Space = injected;
        return _layerHandle;
    }
    

    public event Action PropertiesChangeEvent;
    public virtual IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Title",
            PropertyType = PropertyType.Text,
            Getter = () => title,
            Setter = null
        };
    }

    public abstract float GetReferenceZ();
    public virtual bool CheckOverlap (Vector2 pos) => false;
    
    public abstract string Pack();
    public abstract void Unpack(string data);

    public virtual void Activate() { }

    public void Clear()
    {
        var targetObject = target.gameObject;
        Destroy(this);
        Destroy(targetObject);
    }

    public abstract void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove);

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected void InvokePropertiesChangeEvent() => PropertiesChangeEvent?.Invoke();
}
}