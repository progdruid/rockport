using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggeredType
{
    None,
    Wall,
    Player,
    Corpse
}

[RequireComponent(typeof(Collider2D))]
public class UniversalTrigger : MonoBehaviour
{
    public event System.Action<Collider2D, TriggeredType> EnterEvent = delegate { };
    public event System.Action<Collider2D, TriggeredType> ExitEvent = delegate { };

    protected void InvokeEnterEvent(Collider2D other, TriggeredType type) => EnterEvent(other, type);
    protected void InvokeExitEvent(Collider2D other, TriggeredType type) => ExitEvent(other, type);

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {   
        TriggeredType type = TriggeredType.Wall;
        if (other.tag == "Player")
            type = TriggeredType.Player;
        else if (other.tag == "Corpse")
            type = TriggeredType.Corpse;

        InvokeEnterEvent(other, type);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        TriggeredType type = TriggeredType.Wall;
        if (other.tag == "Player")
            type = TriggeredType.Player;
        else if (other.tag == "Corpse")
            type = TriggeredType.Corpse;

        InvokeExitEvent(other, type);
    }
}