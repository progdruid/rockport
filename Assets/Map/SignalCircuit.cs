using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{
public class SignalCircuit : IPackable
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly HashSet<SignalEmitter> _emitters = new();
    private readonly HashSet<SignalListener> _listeners = new();
    
    private readonly Dictionary<SignalListener, SignalEmitter> _links = new();
    private readonly Dictionary<SignalEmitter, HashSet<SignalListener>> _invertedLinks = new();
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public IEnumerable<SignalEmitter> Emitters => _emitters;
    public IEnumerable<SignalListener> Listeners => _listeners;
    public IReadOnlyDictionary<SignalListener, SignalEmitter> Links => _links;
    
    public void ExtractAndAdd(MapEntity entity)
    {
        foreach (var (_, module) in entity.PublicModules)
            switch (module)
            {
                case SignalEmitter emitter: _emitters.Add(emitter); break;
                case SignalListener listener: _listeners.Add(listener); break;
            }
    }

    public void ExtractAndRemove(MapEntity entity)
    {
        foreach (var (key, module) in entity.PublicModules)
            switch (module)
            {
                case SignalEmitter emitter: _emitters.Remove(emitter); break;
                case SignalListener listener: _listeners.Remove(listener); break;
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

    public string Pack()
    {
        var emitterData = new string[_links.Count];
        var listenerData = new string[_links.Count];
        
        var i = 0;
        foreach (var (listener, emitter) in _links)
        {
            emitterData[i] = ((IEntityModule)emitter).GetModulePath().Pack();
            listenerData[i] = ((IEntityModule)listener).GetModulePath().Pack();
            i++;
        }
        return JsonUtility.ToJson((emitterData, listenerData));
    }

    public void Unpack(string data)
    {
        Dictionary<EntityModulePath, SignalEmitter> emitterMap = new();
        Dictionary<EntityModulePath, SignalListener> listenerMap = new();
        
        foreach (var emitter in _emitters) 
            emitterMap[((IEntityModule)emitter).GetModulePath()] = emitter;
        foreach (var listener in _listeners) 
            listenerMap[((IEntityModule)listener).GetModulePath()] = listener;
        
        var (emitterData, listenerData) = JsonUtility.FromJson<(string[], string[])>(data);
        
        for (var i = 0; i < emitterData.Length; i++)
        {
            var emitterPath = new EntityModulePath();
            var listenerPath = new EntityModulePath();
            emitterPath.Unpack(emitterData[i]);
            listenerPath.Unpack(listenerData[i]);
            
            emitterMap.TryGetValue(emitterPath, out var emitter);
            listenerMap.TryGetValue(listenerPath, out var listener);
            Assert.IsNotNull(emitter);
            Assert.IsNotNull(listener);
            
            Link(emitter, listener);
        }
    }
}

}