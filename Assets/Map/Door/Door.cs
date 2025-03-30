using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Map
{

[RequireComponent(typeof(Animator))]
public class Door : EntityComponent
{
    private static readonly int Open = Animator.StringToHash("Open");

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Door")]
    [SerializeField] private Animator animator;
    [SerializeField] private UnityEvent onOpening;
    [SerializeField] private UnityEvent onClosing;

    private bool _open = false;
    private SignalListener _listener;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        _listener = new SignalListener();
        Entity.AddPublicModule("signal-input-0", _listener);

        Assert.IsNotNull(animator);
    }
    public override void Initialise() { }
    public override void Activate()
    {
        _listener.ActionOnSignal = UpdateDoor;
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "door";
    public override IEnumerator<PropertyHandle> GetProperties() { yield break; }
    public override void Replicate(JSONNode data) { }
    public override JSONNode ExtractData() => new JSONObject();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateDoor(bool activate)
    {
        animator.SetBool(Open, activate);
        if (activate && !_open)
            onOpening.Invoke();
        else if (activate != _open)
            onClosing.Invoke();

        _open = activate;
    }
}

}