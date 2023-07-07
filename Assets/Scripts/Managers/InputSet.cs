using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSet : MonoBehaviour
{
    //game input events
    public event System.Action QuitActivationEvent = delegate { };
    public event System.Action ReloadActivationEvent = delegate { };
    public event System.Action KillPlayerKeyPressEvent = delegate { };
    protected void InvokeQuitActivationEvent() => QuitActivationEvent();
    protected void InvokeReloadActivationEvent() => ReloadActivationEvent();
    protected void InvokeKillPlayerKeyPressEvent() => KillPlayerKeyPressEvent();


    //state update events
    public event System.Action<bool, bool> ActiveUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanWalkUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanJumpUpdateEvent = delegate { };
    

    //player input
    public event System.Action JumpKeyPressEvent = delegate { }; //not to always write if null
    public event System.Action JumpKeyReleaseEvent = delegate { }; //bit stupid, i know
    protected void InvokeJumpKeyPressEvent() => JumpKeyPressEvent();
    protected void InvokeJumpKeyReleaseEvent() => JumpKeyReleaseEvent();

    public float HorizontalValue { get; protected set; }
    public bool HoldingJumpKey { get; protected set; }

    private bool active;
    public bool Active{
        get { return active; }
        set{
            ActiveUpdateEvent(active, value);
            active = value;
            CanWalk = value;
            CanJump = value;
        }
    }

    private bool canWalk = true;
    public bool CanWalk {
        get { return canWalk; }
        set {
            CanWalkUpdateEvent(canWalk, value);
            canWalk = value; 
            if (!value)
                HorizontalValue = 0f;
            
        }
    }

    private bool canJump = true;
    public bool CanJump{
        get { return canJump; }
        set{
            CanJumpUpdateEvent(canJump, value);
            canJump = value;
            if (!value)
                HoldingJumpKey = false;
        }
    }

    void Awake() => Registry.ins.inputSet = this;

    
}
