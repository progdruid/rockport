using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour
{
    [SerializeField] UnityEvent OnOpening;
    [SerializeField] UnityEvent OnClosing;

    public SignalSource signalSource;

    private Animator animator;
    private bool open = false;

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
        if (active && active != open)
            OnOpening.Invoke();
        else if (active != open)
            OnClosing.Invoke();

        open = active;
    }
}
