using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class Checkpoint : MonoBehaviour
{
    private Animator animator;
    private UniversalTrigger trigger;

    private bool activated;

    #region ceremony
    private void Start()
    {
        animator = GetComponent<Animator>();
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
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || activated)
            return;

        activated = true;
        Registry.ins.playerManager.SetSpawnPoint(transform.position);
        animator.SetTrigger("Burned");
    }
}
