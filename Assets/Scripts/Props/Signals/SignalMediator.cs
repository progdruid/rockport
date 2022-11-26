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
        { 
            activators[i].ActivationUpdateEvent += UpdateActivation;
            if (activators[i].activated)
                activeSources.Add(activators[i].gameObject);
        }

        base.UpdateActivation(Calculate(), gameObject);
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

        base.UpdateActivation(Calculate(), gameObject);
    }

    private bool Calculate ()
    {
        bool and = (mode == SignalCombinationMode.AND) && (activeSources.Count == activators.Count);
        bool or = (mode == SignalCombinationMode.OR) && (activeSources.Count > 0);
        return and || or;
    }
}
