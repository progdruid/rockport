using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignalActivator))]
public class Piston : MonoBehaviour
{
    private SignalActivator signal;
    private Animator animator;
    private Collider2D trigger;

    private bool pressed = false;

    private void Start()
    {
        signal = GetComponent<SignalActivator>();
        animator = GetComponent<Animator>();
        trigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool found = other.TryGetComponent(out SignComponent sign);
        if (!found || !sign.HasSign("Body") || pressed)
            return;

        pressed = !pressed;
        animator.SetTrigger("Pressed");
        signal.UpdateActivation(pressed, gameObject);
        trigger.enabled = false;
    }
}
