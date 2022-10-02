using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public event System.Action JumpKeyPressEvent = delegate { }; //to not always write if null
    public event System.Action JumpKeyReleaseEvent = delegate { }; //bit stupid, i know
    public event System.Action KillPlayerKeyPressEvent = delegate { }; 
    public event System.Action InputDisableEvent = delegate { };
    
    public float HorizontalValue { get; private set; }
    public bool HoldingJumpKey { get; private set; }

    private bool active;
    public bool GetActive () => active;
    public void SetActive (bool val)
    {
        if (!val && active)
        {
            HorizontalValue = 0f;
            InputDisableEvent();
        }
        active = val;
    }

    void Awake()
    {
        Registry.ins.inputSystem = this;
    }

    void Update()
    {
        if (!active)
            return;

        HorizontalValue = Input.GetAxisRaw("Horizontal");
        HoldingJumpKey = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space))
            JumpKeyPressEvent();
        else if (Input.GetKeyUp(KeyCode.Space))
            JumpKeyReleaseEvent();
        if (Input.GetKeyDown(KeyCode.E))
            KillPlayerKeyPressEvent();

    }
}
