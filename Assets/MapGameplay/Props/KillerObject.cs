using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class KillerObject : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    [SerializeField] private UniversalTrigger trigger;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(trigger);
        
        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }

    private void HandleTriggerEnter (Collider2D col, TriggeredType type)
    {
        if (type != TriggeredType.Player)
            return;

        GameSystems.Ins.PlayerManager.KillPlayer();
    }
}
