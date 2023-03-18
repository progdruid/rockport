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

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {   
        TriggeredType type = TriggeredType.Wall;
        if (other.tag == "Player")
            type = TriggeredType.Player;
        else if (other.tag == "Corpse")
            type = TriggeredType.Corpse;
            
        EnterEvent(other, type);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        TriggeredType type = TriggeredType.Wall;
        if (other.tag == "Player")
            type = TriggeredType.Player;
        else if (other.tag == "Corpse")
            type = TriggeredType.Corpse;

        ExitEvent(other, type);
    }
}