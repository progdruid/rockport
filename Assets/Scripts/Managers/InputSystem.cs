using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public event System.Action JumpKeyPressEvent = delegate { }; //to not always write if null
    public event System.Action JumpKeyReleaseEvent = delegate { }; //bit stupid, i know
    public event System.Action KillPlayerKeyPressEvent = delegate { }; 
    public event System.Action<bool, bool> ActiveUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanWalkUpdateEvent = delegate { };
    public event System.Action<bool, bool> CanJumpUpdateEvent = delegate { };
    
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

    void OnEnable() => Registry.ins.inputSystem = this;

    void Update()
    {
        if (!active)
            return;

        HorizontalValue = 0f;
        HoldingJumpKey = false;

        if (CanWalk)
            HorizontalValue = Input.GetAxisRaw("Horizontal");
        if (CanJump)
            HoldingJumpKey = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
            JumpKeyPressEvent();
        else if (Input.GetKeyUp(KeyCode.Space))
            JumpKeyReleaseEvent();

        if (Input.GetKeyDown(KeyCode.E))
            KillPlayerKeyPressEvent();

    }
}
