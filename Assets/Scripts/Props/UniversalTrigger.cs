using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggeredType
{
    None,
    Dirt,
    Wood,
    Player,
    Corpse
}

public class UniversalTrigger : MonoBehaviour
{
    public event System.Action<Collider2D, TriggeredType> EnterEvent = delegate { };
    public event System.Action<Collider2D, TriggeredType> ExitEvent = delegate { };

    private Dictionary<string, TriggeredType> tagTypePairs = new()
    {
        { "Dirt", TriggeredType.Dirt },
        { "Wood", TriggeredType.Wood },
        { "Player", TriggeredType.Player },
        { "Corpse", TriggeredType.Corpse }
    };

    protected void InvokeEnterEvent(Collider2D other, TriggeredType type) => EnterEvent(other, type);
    protected void InvokeExitEvent(Collider2D other, TriggeredType type) => ExitEvent(other, type);

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        TriggeredType type = tagTypePairs[other.tag];

        InvokeEnterEvent(other, type);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        TriggeredType type = tagTypePairs[other.tag];

        InvokeExitEvent(other, type);
    }
}