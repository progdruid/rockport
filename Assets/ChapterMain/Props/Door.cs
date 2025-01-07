using ChapterEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class Door : PropEntity
{
    private static readonly int Open = Animator.StringToHash("Open");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Animator animator;
    [SerializeField] private SignalListener listener;
    [SerializeField] private UnityEvent onOpening;
    [SerializeField] private UnityEvent onClosing;
    
    private bool _open = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        Assert.IsNotNull(animator);
        Assert.IsNotNull(listener);

        listener.ActionOnSignal = UpdateDoor;
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateDoor (bool activate)
    {
        animator.SetBool(Open, activate);
        if (activate && !_open)
            onOpening.Invoke();
        else if (activate != _open)
            onClosing.Invoke();

        _open = activate;
    }
}
