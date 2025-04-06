using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Map
{

public class Lever : EntityComponent
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

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        _signalEmitter = new SignalEmitter { Signal = false };
        Entity.AddAccessor("signal-output", _signalEmitter);

        Assert.IsNotNull(animator);
        Assert.IsNotNull(trigger);

        trigger.EnterEvent += HandleTriggerEnter;
    }

    public override void Initialise() { }
    public override void Activate() { }


    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "lever";
    public override IEnumerator<PropertyHandle> GetProperties() { yield break; }
    public override void Replicate(JSONNode data) { }
    public override JSONNode ExtractData() => new JSONObject();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
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