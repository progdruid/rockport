using System;
using System.Collections.Generic;
using ChapterEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class Button : PropEntity
{
    private static readonly int PressedAnimatorParameterID = Animator.StringToHash("Pressed");
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField ]private Animator animator;
    [SerializeField] private UniversalTrigger trigger;
    [SerializeField] private SignalEmitter emitter;
    [SerializeField] private UnityEvent onPress;
    [SerializeField] private UnityEvent onRelease;
    
    private readonly List<Collider2D> _pressingBodies = new();
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        
        Assert.IsNotNull(animator);
        Assert.IsNotNull(trigger);
        Assert.IsNotNull(emitter);
    
        trigger.EnterEvent += HandleTriggerEnter;
        trigger.ExitEvent += HandleTriggerExit;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        trigger.ExitEvent -= HandleTriggerExit;
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || _pressingBodies.Contains(other))
            return;
        
        _pressingBodies.Add(other);
        if (_pressingBodies.Count > 1)
            return;
        onPress.Invoke();
        animator.SetBool(PressedAnimatorParameterID, true);
        emitter.Signal = true;
    }

    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if (type is not (TriggeredType.Player or TriggeredType.Corpse) || !_pressingBodies.Contains(other))
            return;

        _pressingBodies.Remove(other);
        if (_pressingBodies.Count > 0)
            return;
        onRelease.Invoke();
        animator.SetBool(PressedAnimatorParameterID, false);
        emitter.Signal = false;
    }
}
