using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    //game input events
    public event System.Action QuitActivationEvent = delegate { };
    public event System.Action ReloadActivationEvent = delegate { };
    public event System.Action KillPlayerKeyPressEvent = delegate { }; 

    //state update events
    public event System.Action<bool, bool> ActiveUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanWalkUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanJumpUpdateEvent = delegate { };
    
    //player input
    public event System.Action JumpKeyPressEvent = delegate { }; //not to always write if null
    public event System.Action JumpKeyReleaseEvent = delegate { }; //bit stupid, i know
    public float HorizontalValue { get; private set; }
    public bool HoldingJumpKey { get; private set; }

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

    void Awake() => Registry.ins.inputSystem = this;

    void Update()
    {
        if (!active)
            return;

        if (CanWalk)
            HorizontalValue = Input.GetAxisRaw("Horizontal");
        if (CanJump)
            HoldingJumpKey = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
            JumpKeyPressEvent();
        else if (Input.GetKeyUp(KeyCode.Space))
            JumpKeyReleaseEvent();

        //player kill key is temporary
        //will not be in the final game
        //exists only for testing
        if (Input.GetKeyDown(KeyCode.E))
            KillPlayerKeyPressEvent();
        if (Input.GetKeyDown(KeyCode.R))
            ReloadActivationEvent();
        if (Input.GetKeyDown(KeyCode.Q))
            QuitActivationEvent();

    }
}
