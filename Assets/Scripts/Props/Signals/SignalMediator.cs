using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalMediator : SignalActivator
{
    private enum SignalCombinationMode
    {
        AND,
        OR,
        XOR
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
        if (active && !activeSources.Contains(source)) activeSources.Add(source);
        if (!active) activeSources.Remove(source);

        base.UpdateActivation(Calculate(), gameObject);
    }

    private bool Calculate ()
    {
        bool res = false;

        if (mode == SignalCombinationMode.AND)
            res = activeSources.Count == activators.Count;
        else if (mode == SignalCombinationMode.OR)
            res = activeSources.Count > 0;
        else if (mode == SignalCombinationMode.XOR)
            res = activeSources.Count > 0 && activeSources.Count < activators.Count;

        return res;
    }
}
