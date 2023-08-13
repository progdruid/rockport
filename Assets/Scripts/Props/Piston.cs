using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniversalTrigger), typeof(SignalSource), typeof(Animator))]
public class Piston : MonoBehaviour
{
    [SerializeField] UnityEvent OnPress;

    private SignalSource signal;
    private Animator animator;
    private Collider2D triggerCollider;
    private UniversalTrigger trigger;

    private bool pressed = false;

    #region ceremony
    private void Start()
    {
        signal = GetComponent<SignalSource>();
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
        OnPress.Invoke();
        animator.SetTrigger("Pressed");
        signal.UpdateSignal(pressed, gameObject);
        triggerCollider.enabled = false;
    }
}
