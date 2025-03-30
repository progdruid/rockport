using System.Collections.Generic;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{
public class SignalCircuit : IReplicable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly HashSet<SignalEmitter> _emitters = new();
    private readonly HashSet<SignalListener> _listeners = new();
    
    private readonly Dictionary<SignalListener, SignalEmitter> _links = new();
    private readonly Dictionary<SignalEmitter, HashSet<SignalListener>> _invertedLinks = new();
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public IReadOnlyCollection<SignalEmitter> Emitters => _emitters;
    public IReadOnlyCollection<SignalListener> Listeners => _listeners;
    public IReadOnlyDictionary<SignalListener, SignalEmitter> Links => _links;
    public bool TryGetLinks(SignalEmitter emitter, out IReadOnlyCollection<SignalListener> listeners)
    {
        if (_invertedLinks.TryGetValue(emitter, out var outListeners))
        {
            listeners = outListeners;
            return true;
        }
        listeners = null;
        return false;
    }
    
    
    public void ExtractAndAdd(MapEntity entity)
    {
        foreach (var (_, module) in entity.Accessors)
            switch (module)
            {
                case SignalEmitter emitter: _emitters.Add(emitter); break;
                case SignalListener listener: _listeners.Add(listener); break;
            }
    }

    public void ExtractAndRemove(MapEntity entity)
    {
        foreach (var (key, module) in entity.Accessors)
            switch (module)
            {
                case SignalEmitter emitter: 
                    Unlink(emitter); 
                    _emitters.Remove(emitter); 
                    break;
                case SignalListener listener: 
                    Unlink(listener);
                    _listeners.Remove(listener); 
                    break;
            }
    }

    public void Link(SignalEmitter emitter, SignalListener listener)
    {
        Unlink(listener);
        emitter.AddListener(listener);
        _invertedLinks.TryAdd(emitter, new HashSet<SignalListener>());
        _invertedLinks[emitter].Add(listener);
        _links.Add(listener, emitter);
    }

    public void Unlink(SignalListener listener)
    {
        if (!_links.TryGetValue(listener, out var previousEmitter)) 
            return;
        
        previousEmitter.RemoveListener(listener);
        _invertedLinks[previousEmitter].Remove(listener);
        _links.Remove(listener);
    }
    
    public void Unlink(SignalEmitter emitter)
    {
        if (!_invertedLinks.TryGetValue(emitter, out var linkedListeners))
            return;

        foreach (var listener in linkedListeners)
        {
            emitter.RemoveListener(listener);
            _links.Remove(listener);
        }
        
        _invertedLinks.Remove(emitter);
    }

    public JSONNode ExtractData()
    {
        var json = new JSONObject();
        var emitterData = new JSONArray();
        var listenerData = new JSONArray();

        foreach (var (listener, emitter) in _links)
        {
            emitterData.Add(((IEntityAccessor)emitter).GetAccessorPath().ExtractData());
            listenerData.Add(((IEntityAccessor)listener).GetAccessorPath().ExtractData());
        }

        json["emitterData"] = emitterData;
        json["listenerData"] = listenerData;
        return json;
    }

    public void Replicate(JSONNode data)
    {
        Dictionary<EntityAccessorPath, SignalEmitter> emitterMap = new();
        Dictionary<EntityAccessorPath, SignalListener> listenerMap = new();

        foreach (var emitter in _emitters)
            emitterMap[((IEntityAccessor)emitter).GetAccessorPath()] = emitter;
        foreach (var listener in _listeners)
            listenerMap[((IEntityAccessor)listener).GetAccessorPath()] = listener;

        var emitterData = data["emitterData"].AsArray;
        var listenerData = data["listenerData"].AsArray;

        for (var i = 0; i < emitterData.Count; i++)
        {
            var emitterPath = new EntityAccessorPath();
            var listenerPath = new EntityAccessorPath();
            emitterPath.Replicate(emitterData[i]);
            listenerPath.Replicate(listenerData[i]);

            emitterMap.TryGetValue(emitterPath, out var emitter);
            listenerMap.TryGetValue(listenerPath, out var listener);
            Assert.IsNotNull(emitter);
            Assert.IsNotNull(listener);

            Link(emitter, listener);
        }
    }
}

}