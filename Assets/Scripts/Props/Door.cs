using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour
{
    public SignalSource signalSource;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        signalSource.SignalUpdateEvent += UpdateDoor;
    }

    private void OnDestroy()
    {
        signalSource.SignalUpdateEvent -= UpdateDoor;
    }

    private void UpdateDoor (bool active, GameObject source)
    {
        animator.SetBool("Open", active);
    }
}
