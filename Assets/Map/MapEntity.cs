using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public abstract class MapEntity : MonoBehaviour, IPropertyHolder, IReplicable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Entity Base")]
    [SerializeField] private string title;
    [SerializeField] private Transform target;
    
    protected MapSpace Space { get; private set; }
    private readonly IntReference _layerHandle = new();
    private readonly Dictionary<string, IEntityModule> _publicModules = new();
    
    private bool _initialised = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Always call base.Awake() in derived classes.
    /// </summary>
    protected virtual void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(title);
        Assert.IsTrue(title.Length > 0);
    }
    
    protected void Start() => EnsureInitialise();
    
    /// <summary>
    /// You can manually ensure initialisation.
    /// </summary>
    protected void EnsureInitialise()
    {
        if (_initialised) return;
        _initialised = true;
        Initialise();
    }

    /// <summary>
    /// This is "Start". Use this for any setup that requires external dependencies to be ready.
    /// </summary>
    protected virtual void Initialise() { }
    
    /// <summary>
    /// Called when gameplay mode starts.
    /// </summary>
    public virtual void Activate() { }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string Title => title;
    public Transform Target => target;
    public int Layer => _layerHandle.Value;
    public IReadOnlyDictionary<string, IEntityModule> PublicModules => _publicModules;
    
    
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

    public virtual bool CheckOverlap (Vector2 pos) => false;
    public virtual float GetReferenceZ() => Target.position.z;
    public virtual Vector2Int GetOverlayAnchor()
    {
        var snapped = Space.SnapWorldToMap(Target.position, out var anchorPoint);
        Assert.IsTrue(snapped);
        return anchorPoint;
    }

    public abstract JSONNode ExtractData();
    public abstract void Replicate(JSONNode data);

    /// <summary>
    /// Deletes the entity together with its GameObject.
    /// </summary>
    public void Clear()
    {
        var targetObject = target.gameObject;
        Destroy(this);
        Destroy(targetObject);
    }

    public abstract void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove);
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    protected void InvokePropertiesChangeEvent() => PropertiesChangeEvent?.Invoke();
    protected void AddPublicModule(string moduleName, IEntityModule module)
    {
        _publicModules.TryAdd(moduleName, module);
        module.Initialise(moduleName, this);
    }
}
}