using System.Collections.Generic;
using Common;

namespace ChapterEditor
{

public class SignalEmitter : IEntityModule
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly HashSet<SignalListener> _listeners = new();
    private bool _signal;
    
    MapEntity IEntityModule.Entity { get; set; }
    string IEntityModule.ModuleName { get; set; }
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public bool Signal
    {
        get => _signal;
        set
        {
            if (_signal == value) return;
            _signal = value;
            foreach (var listener in _listeners) 
                listener.ReceiveSignal(value);
        }
    }

    public void AddListener(SignalListener listener)
    {
        _listeners.Add(listener);
        listener.ReceiveSignal(_signal);
    }

    public void RemoveListener(SignalListener listener)
    {
        _listeners.Remove(listener);
        listener.ReceiveSignal(false);
    }
    
    
}

}