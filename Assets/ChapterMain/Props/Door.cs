using ChapterEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Door : PropEntity
{
    private static readonly int Open = Animator.StringToHash("Open");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Animator animator;
    [SerializeField] private UnityEvent onOpening;
    [SerializeField] private UnityEvent onClosing;
    
    private bool _open = false;
    private SignalListener _listener;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        _listener = new SignalListener();
        AddPublicModule("signal-input-0", _listener);
        
        base.Awake();
        
        Assert.IsNotNull(animator);
    }

    public override void Activate()
    {
        base.Activate();
        _listener.ActionOnSignal = UpdateDoor;
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
