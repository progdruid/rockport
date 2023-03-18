using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class CollectibleSkull : MonoBehaviour
{
    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }
    #endregion

    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            Collect();
    }

    //suppesed to be a coroutine due to animation,
    //but because there is no animation yet, it is just a method
    private void Collect ()
    {
        Registry.ins.skullManager.AddSkull();
        //yield animation
        Destroy(gameObject);
    }
}
