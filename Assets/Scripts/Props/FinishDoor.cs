using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class FinishDoor : MonoBehaviour
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

    private void HandleTriggerEnter (Collider2D col, TriggeredType type)
    {
        if (type != TriggeredType.Player)
            return;

        Registry.ins.lm.ProceedFurther();
    }
}
