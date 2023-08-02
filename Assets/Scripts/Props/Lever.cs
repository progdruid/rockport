using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger), typeof(SignalSource), typeof(Animator))]
public class Lever : MonoBehaviour
{
    private bool pulled;

    private SignalSource signal;
    private Animator animator;
    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        signal = GetComponent<SignalSource>();
        animator = GetComponent<Animator>();
        trigger = GetComponent<UniversalTrigger>();

        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }
    #endregion

    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if (type != TriggeredType.Player && type != TriggeredType.Corpse)
            return;

        pulled = !pulled;
        animator.SetTrigger("Pulled");
        signal.UpdateSignal(pulled, gameObject);
    }
}
