using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalNegator : SignalSource
{
    [SerializeField] SignalSource signal;

    private void Start()
    {
        signal.SignalUpdateEvent += UpdateSignal;
        UpdateSignal(signal.Activated, signal.gameObject);
    }

    private void OnDestroy()
    {
        signal.SignalUpdateEvent -= UpdateSignal;
    }

    public override void UpdateSignal(bool active, GameObject source)
    {
        base.UpdateSignal(!active, source);
    }
}
