using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class PlatformTrigger : MonoBehaviour
{
    public Collider2D platformCollider;

    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        trigger.EnterEvent += HandleTriggerEnter;
        trigger.ExitEvent += HandleTriggerExit;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        trigger.ExitEvent -= HandleTriggerExit;
    }
    #endregion

    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
            Physics2D.IgnoreCollision(other, platformCollider, false);
    }

    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
            Physics2D.IgnoreCollision(other, platformCollider, true);
    }
}
