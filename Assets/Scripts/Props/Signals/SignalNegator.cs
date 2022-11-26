using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalNegator : SignalActivator
{
    [SerializeField] SignalActivator signal;

    private void Start()
    {
        signal.ActivationUpdateEvent += UpdateActivation;
    }

    private void OnDestroy()
    {
        signal.ActivationUpdateEvent -= UpdateActivation;
    }

    public override void UpdateActivation(bool active, GameObject source)
    {
        base.UpdateActivation(!active, source);
    }
}
