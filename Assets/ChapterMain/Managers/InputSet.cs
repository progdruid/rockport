using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSet : MonoBehaviour
{
    //game input events
    public event System.Action QuitActivationEvent = delegate { }; //not to always write if null
    public event System.Action ReloadActivationEvent = delegate { }; //bit stupid, i know
    public event System.Action KillPlayerKeyPressEvent = delegate { };

    public void InvokeQuitActivationEvent() => QuitActivationEvent();
    public void InvokeReloadActivationEvent() => ReloadActivationEvent();
    public void InvokeKillPlayerKeyPressEvent()
    {
        if (Active)
            KillPlayerKeyPressEvent();
    }

    //state update events
    public event System.Action<bool, bool> ActiveUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanWalkUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanJumpUpdateEvent = delegate { };
    

    //player input
    public event System.Action JumpKeyPressEvent = delegate { }; 
    public event System.Action JumpKeyReleaseEvent = delegate { }; 
    protected void InvokeJumpKeyPressEvent() 
    {
        if (Active && CanJump)
            JumpKeyPressEvent(); 
    }
    protected void InvokeJumpKeyReleaseEvent()
    {
        if (Active && CanJump)
            JumpKeyReleaseEvent();
    }

    private float hValue = 0f;
    public float HorizontalValue 
    {
        get { return hValue; }
        protected set
        {
            if (Active && CanWalk)
                hValue = value;
            else hValue = 0f;
        } 
    }

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
        }
    }

    void Awake()
    {
        if (enabled)
            GameSystems.ins.inputSet = this;
    }
}
