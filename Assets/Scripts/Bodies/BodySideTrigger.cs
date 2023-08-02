using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySideTrigger : UniversalTrigger
{
    public bool triggered => playerTriggered || corpseTriggered || wallTriggered;
    public bool playerTriggered => typeListMap[TriggeredType.Player].Count == 1;
    public bool corpseTriggered => typeListMap[TriggeredType.Corpse].Count > 0;
    public bool wallTriggered => woodTriggered || dirtTriggered;
    public bool dirtTriggered => typeListMap[TriggeredType.Dirt].Count > 0;
    public bool woodTriggered => typeListMap[TriggeredType.Wood].Count > 0;

    public Rigidbody2D body { get; private set; }

    private Dictionary<TriggeredType, List<Collider2D>> typeListMap = new()
    {
        {TriggeredType.Dirt, new () },
        {TriggeredType.Wood, new () },
        {TriggeredType.Corpse, new () },
        {TriggeredType.Player, new () }
    };

    private void HandleEnter(Collider2D other, TriggeredType type)
    {
        var list = typeListMap[type];
        if (!list.Contains(other))
        {
            list.Add(other);
            bool got = other.TryGetComponent<Rigidbody2D>(out var receivedBody);
            if (got) body = receivedBody;
        }
    }
    
    private void HandleExit(Collider2D other, TriggeredType type)
    {
        var list = typeListMap[type];
        if (list.Contains(other))
        {
            list.Remove(other);
            bool got = other.TryGetComponent<Rigidbody2D>(out var receivedBody);
            if (got && body == receivedBody) body = null;
        }
    }

    protected override void OnTriggerEnter2D (Collider2D other)
    {
        TriggeredType type = tagTypeMap[other.tag];

        HandleEnter(other, type);

        InvokeEnterEvent(other, type);
    }
    protected override void OnTriggerExit2D (Collider2D other)
    {
        TriggeredType type = tagTypeMap[other.tag];

        HandleExit(other, type);

        InvokeExitEvent(other, type);
    }
}
