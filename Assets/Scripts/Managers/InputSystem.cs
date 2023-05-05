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

        //temporary
        //will not be in the final game
        //exists only for testing
        if (Input.GetKeyDown(KeyCode.E))
            KillPlayerKeyPressEvent();
        if (Input.GetKeyDown(KeyCode.R))
            Registry.ins.lm.ReloadLevel();

    }
}
