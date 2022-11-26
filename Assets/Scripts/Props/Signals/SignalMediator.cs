using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalMediator : SignalActivator
{
    private enum SignalCombinationMode
    {
        AND,
        OR
    }

    [SerializeField] SignalCombinationMode mode;
    [SerializeField] List<SignalActivator> activators;
    private List<GameObject> activeSources;

    private void Start()
    {
        activeSources = new List<GameObject>();

        for (int i = 0; i < activators.Count; i++)
            activators[i].ActivationUpdateEvent += UpdateActivation;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < activators.Count; i++)
            activators[i].ActivationUpdateEvent -= UpdateActivation;
    }

    public override void UpdateActivation(bool active, GameObject source)
    {
        if (active) activeSources.Add(source);
        else activeSources.Remove(source);

        if (mode == SignalCombinationMode.AND)
            base.UpdateActivation(activeSources.Count == activators.Count, gameObject); //lol
        else if (mode == SignalCombinationMode.OR)
            base.UpdateActivation(activeSources.Count > 0, gameObject);
    }
}
