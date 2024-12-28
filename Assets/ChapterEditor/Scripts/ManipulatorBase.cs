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
    protected MapSpaceHolder Holder { get; private set; }

    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected virtual void Awake() => Assert.IsNotNull(target);
    protected void Start() => RequestInitialise();
    protected virtual void Initialise() {}

    protected void RequestInitialise()
    {
        if (_initialised) return;
        _initialised = true;
        Initialise();
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string ManipulatorName => manipulatorName;
    public Transform Target => target;

    public event Action PropertiesChangeEvent;

    public void InjectHolder(MapSpaceHolder injected) => Holder = injected;

    
    public virtual IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Title",
            PropertyType = PropertyType.Text,
            Getter = () => manipulatorName,
            Setter = (object input) => manipulatorName = (string)input
        };
    }

    public abstract float GetReferenceZ();
    public virtual bool CheckOverlap (Vector2 pos) => false;
    
    public abstract string Pack();
    public abstract void Unpack(string data);

    public void Release()
    {
        HandleRelease();
        Destroy(this);
    }
    public void Clear()
    {
        var targetObject = target.gameObject;
        Destroy(this);
        Destroy(targetObject);
    }

    public abstract void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove);

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected void InvokePropertiesChangeEvent() => PropertiesChangeEvent?.Invoke();

    protected virtual void HandleRelease() { }
}

}