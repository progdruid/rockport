using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniversalTrigger), typeof(SignalSource), typeof(Animator))]
public class Button : MonoBehaviour
{
    [SerializeField] UnityEvent OnPress;
    [SerializeField] UnityEvent OnUnpress;

    private List<Collider2D> pressingBodies = new List<Collider2D>();

    private Animator animator;
    private SignalSource signal;
    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        animator = GetComponent<Animator>();
        signal = GetComponent<SignalSource>();
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

    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || pressingBodies.Contains(other))
            return;
        
        pressingBodies.Add(other);
        if (pressingBodies.Count > 1)
            return;
        OnPress.Invoke();
        animator.SetBool("Pressed", true);
        signal.UpdateSignal(true, gameObject);
    }

    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || !pressingBodies.Contains(other))
            return;

        pressingBodies.Remove(other);
        if (pressingBodies.Count > 0)
            return;
        OnUnpress.Invoke();
        animator.SetBool("Pressed", false);
        signal.UpdateSignal(false, gameObject);
    }
}
