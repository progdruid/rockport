using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Map
{

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class Button : EntityComponent
{
    private static readonly int PressedAnimatorParameterID = Animator.StringToHash("Pressed");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Animator animator;
    [SerializeField] private UniversalTrigger trigger;
    [SerializeField] private UnityEvent onPress;
    [SerializeField] private UnityEvent onRelease;

    private readonly List<Collider2D> _pressingBodies = new();
    private SignalEmitter _signalEmitter;

    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        _signalEmitter = new SignalEmitter { Signal = false };
        Entity.AddPublicModule("signal-output", _signalEmitter);

        Assert.IsNotNull(animator);
        Assert.IsNotNull(trigger);

        trigger.EnterEvent += HandleTriggerEnter;
        trigger.ExitEvent += HandleTriggerExit;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        trigger.ExitEvent -= HandleTriggerExit;
    }

    public override void Initialise() {}
    public override void Activate() {}

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "button";
    public override IEnumerator<PropertyHandle> GetProperties() { yield break; }
    public override void Replicate(JSONNode data) { }
    public override JSONNode ExtractData() => new JSONObject();
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || _pressingBodies.Contains(other))
            return;

        _pressingBodies.Add(other);
        if (_pressingBodies.Count > 1)
            return;
        onPress.Invoke();
        animator.SetBool(PressedAnimatorParameterID, true);
        _signalEmitter.Signal = true;
    }

    private void HandleTriggerExit(Collider2D other, TriggeredType type)
    {
        if (type is not (TriggeredType.Player or TriggeredType.Corpse) || !_pressingBodies.Contains(other))
            return;

        _pressingBodies.Remove(other);
        if (_pressingBodies.Count > 0)
            return;
        onRelease.Invoke();
        animator.SetBool(PressedAnimatorParameterID, false);
        _signalEmitter.Signal = false;
    }
}

}