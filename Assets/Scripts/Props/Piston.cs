using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignalActivator), typeof(UniversalTrigger))]
public class Piston : MonoBehaviour
{
    private SignalActivator signal;
    private Animator animator;
    private Collider2D triggerCollider;
    private UniversalTrigger trigger;

    private bool pressed = false;

    #region ceremony
    private void Start()
    {
        signal = GetComponent<SignalActivator>();
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<Collider2D>();
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
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || pressed)
            return;

        pressed = !pressed;
        animator.SetTrigger("Pressed");
        signal.UpdateActivation(pressed, gameObject);
        triggerCollider.enabled = false;
    }
}
