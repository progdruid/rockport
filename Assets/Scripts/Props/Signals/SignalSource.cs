using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalSource : MonoBehaviour
{
    [SerializeField] bool Output;
    public bool activated { get; private set; }
    public event System.Action<bool, GameObject> SignalUpdateEvent = delegate { };

    private void Start()
    {
        SignalUpdateEvent(activated, gameObject);
    }

    private void OnDestroy()
    {
        SignalUpdateEvent(false, gameObject);
    }

    public virtual void UpdateSignal (bool active, GameObject source)
    {
        activated = active;
        Output = active;
        SignalUpdateEvent (active, source);
    }
}
