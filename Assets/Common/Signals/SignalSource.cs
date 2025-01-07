using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SignalSource : MonoBehaviour
{
    private static HashSet<SignalSource> SignalsInScene = new HashSet<SignalSource>();
    
    [SerializeField] private bool output;
    public bool Activated { get; private set; }
    public event System.Action<bool, GameObject> SignalUpdateEvent;

    private void Start()
    {
        SignalUpdateEvent?.Invoke(Activated, gameObject);
    }

    private void OnDestroy()
    {
        SignalUpdateEvent?.Invoke(false, gameObject);
    }

    public virtual void UpdateSignal (bool active, GameObject source)
    {
        Activated = active;
        output = active;
        SignalUpdateEvent?.Invoke(active, source);
    }
}
