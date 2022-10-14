using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignalActivator))]
public class Button : MonoBehaviour
{
    private List<SignComponent> pressingBodies = new List<SignComponent>();
    private Animator animator;
    private SignalActivator activator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        activator = GetComponent<SignalActivator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool found = other.TryGetComponent(out SignComponent sign);
        bool valid = found && sign.HasSign("Body") && !pressingBodies.Contains(sign);
        if (!valid)
            return;
        
        pressingBodies.Add(sign);
        if (pressingBodies.Count > 1)
            return;
        animator.SetBool("Pressed", true);
        activator.UpdateActivation(true, gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool found = other.TryGetComponent(out SignComponent sign);
        bool valid = found && sign.HasSign("Body") && pressingBodies.Contains(sign);
        if (!valid)
            return;

        pressingBodies.Remove(sign);
        if (pressingBodies.Count > 0)
            return;
        animator.SetBool("Pressed", false);
        activator.UpdateActivation(false, gameObject);
    }
}
