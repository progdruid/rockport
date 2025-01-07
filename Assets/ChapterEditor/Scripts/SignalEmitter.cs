using System.Collections.Generic;
using UnityEngine;

namespace ChapterEditor
{

public class SignalEmitter : MonoBehaviour
{
    private readonly HashSet<SignalListener> _listeners = new();
    private bool _signal;

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

    public void AddListener(SignalListener listener) => _listeners.Add(listener);
    public void RemoveListener(SignalListener listener) => _listeners.Remove(listener);
}

}