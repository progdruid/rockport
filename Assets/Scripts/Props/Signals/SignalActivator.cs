using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalActivator : MonoBehaviour
{
    public bool activated { get; private set; }
    public event System.Action<bool, GameObject> ActivationUpdateEvent = delegate { };

    private void Start()
    {
        ActivationUpdateEvent(activated, gameObject);
    }

    private void OnDestroy()
    {
        ActivationUpdateEvent(false, gameObject);
    }

    public virtual void UpdateActivation (bool active, GameObject source)
    {
        activated = active;
        ActivationUpdateEvent (active, source);
    }
}
