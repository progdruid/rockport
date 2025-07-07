using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Map
{

public abstract class EntityComponent : MonoBehaviour, IReplicable, IPropertyHolder
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    protected MapEntity Entity { get; private set; }
    protected Transform Target { get; private set; }
    protected MapSpace Space { get; private set; }
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Setup(MapEntity entity, Transform target)
    {
        Entity = entity;
        Target = target;
        Wake();
    }
    protected abstract void Wake();
    public void InjectMap(MapSpace injected) => Space = injected;
    public abstract void Initialise();
    public abstract void Activate();
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public abstract string JsonName { get; }
    public event Action PropertiesChangeEvent;
    public abstract IEnumerator<PropertyHandle> GetProperties();
    //todo: rename to more meaningful name
    protected void InvokePropertiesChangeEvent() => PropertiesChangeEvent?.Invoke();
    
    public abstract void Replicate(JSONNode data);
    public abstract JSONNode ExtractData();
    
}

}