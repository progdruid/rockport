using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Map
{

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class Lever : PropEntity
{
    private static readonly int PulledAnimatorParameterID = Animator.StringToHash("Pulled");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Lever")]
    [SerializeField] private Animator animator;
    [SerializeField] private UniversalTrigger trigger;
    [SerializeField] private UnityEvent onPull;

    private readonly List<Collider2D> _pressingBodies = new();
    private SignalEmitter _signalEmitter;
    
    private bool _pulled;

    protected override void Awake()
    {
        _signalEmitter = new SignalEmitter { Signal = false };
        AddPublicModule("signal-output", _signalEmitter);

        base.Awake();

        Assert.IsNotNull(animator);
        Assert.IsNotNull(trigger);

        trigger.EnterEvent += HandleTriggerEnter;
    }
    
    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }


    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type != TriggeredType.Player && type != TriggeredType.Corpse)
            return;

        _pulled = !_pulled;
        onPull.Invoke();
        animator.SetTrigger(PulledAnimatorParameterID);
        _signalEmitter.Signal = _pulled;
    }
}

}