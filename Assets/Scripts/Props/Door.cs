using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public SignalActivator signalSource;

    private Animator animator;
    private new BoxCollider2D collider;

    private bool open;

    private void Start()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();

        signalSource.ActivationUpdateEvent += UpdateDoor;
    }

    private void OnDestroy()
    {
        signalSource.ActivationUpdateEvent -= UpdateDoor;
    }

    private void UpdateDoor (bool active, GameObject source)
    {
        open = active;
        animator.SetBool("Open", active);
    }
}
