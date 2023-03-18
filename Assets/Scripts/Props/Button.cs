using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignalActivator), typeof(UniversalTrigger))]
public class Button : MonoBehaviour
{
    private List<Collider2D> pressingBodies = new List<Collider2D>();

    private Animator animator;
    private SignalActivator activator;
    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        animator = GetComponent<Animator>();
        activator = GetComponent<SignalActivator>();
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
        animator.SetBool("Pressed", true);
        activator.UpdateActivation(true, gameObject);
    }

    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || !pressingBodies.Contains(other))
            return;

        pressingBodies.Remove(other);
        if (pressingBodies.Count > 0)
            return;
        animator.SetBool("Pressed", false);
        activator.UpdateActivation(false, gameObject);
    }
}
