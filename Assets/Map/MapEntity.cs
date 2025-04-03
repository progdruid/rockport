using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class MapEntity : MonoBehaviour, IPropertyHolder, IReplicable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Entity Base")]
    [SerializeField] private string title;
    [SerializeField] private Transform target;
    [SerializeField] private EntityComponent[] components;

    private MapSpace Space { get; set; }
    private readonly IntReference _layerHandle = new();
    private readonly Dictionary<string, IEntityAccessor> _accessors = new();
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(title);
        Assert.IsTrue(title.Length > 0);
        
        foreach (var component in components) 
            component.Setup(this, target);
    }
    
    public IntReference InjectMap(MapSpace injected)
    {
        Space = injected;
        foreach (var component in components)
            component.InjectMap(Space);
        return _layerHandle;
    }
    
    public void Initialise()
    { 
        foreach (var component in components) 
            component.Initialise();
    }

    /// <summary>
    /// Called when gameplay mode starts.
    /// </summary>
    public void Activate()
    {
        foreach (var component in components) 
            component.Activate();
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string Title => title;
    public Transform Target => target;
    public int Layer => _layerHandle.Value;
    public IReadOnlyDictionary<string, IEntityAccessor> Accessors => _accessors;
    

    public event Action PropertiesChangeEvent;
    public IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Title",
            PropertyType = PropertyType.Text,
            Getter = () => title,
            Setter = null
        };

        foreach (var component in components)
        {
            var properties = component.GetProperties();
            while (properties.MoveNext())
                yield return properties.Current;
        }
    }

    /// <summary>
    /// Out of order just now.
    /// </summary>
    public bool CheckOverlap (Vector2 pos) => false;
    public float GetReferenceZ() => Target.position.z;
    public Vector2Int GetOverlayAnchor()
    {
        var snapped = Space.SnapWorldToMap(Target.position, out var anchorPoint);
        Assert.IsTrue(snapped);
        return anchorPoint;
    }

    public JSONNode ExtractData()
    {
        var json = new JSONObject();
        json["title"] = title;
        
        foreach (var component in components)
        {
            var componentName = component.JsonName;
            json[componentName] = component.ExtractData();
        }
        return json;
    }

    public void Replicate(JSONNode data)
    {
        foreach (var component in components)
        {
            var componentName = component.JsonName;
            if (!data.HasKey(componentName)) continue;
            component.Replicate(data[componentName]);
        }
        PropertiesChangeEvent?.Invoke();
    }

    /// <summary>
    /// Deletes the entity together with its GameObject.
    /// </summary>
    public void Clear()
    {
        var targetObject = target.gameObject;
        Destroy(this);
        Destroy(targetObject);
    }
    
    public void AddPublicModule(string moduleName, IEntityAccessor accessor)
    {
        _accessors.TryAdd(moduleName, accessor);
        accessor.Initialise(moduleName, this);
    }
}
}