using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassNullifier : MonoBehaviour
{
    [SerializeField] BodySideTrigger bottomTrigger;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Player player_optional;

    private float defaultMass;

    void Start()
    {
        defaultMass = rb.mass;

        bottomTrigger.EnterEvent += HandleTriggerChange;
        bottomTrigger.ExitEvent += HandleTriggerChange;

        if (player_optional != null)
            player_optional.PreJumpEvent += HandleJump;
    }

    private void OnDestroy()
    {
        bottomTrigger.EnterEvent -= HandleTriggerChange;
        bottomTrigger.ExitEvent -= HandleTriggerChange;

        if (player_optional != null)
            player_optional.PreJumpEvent -= HandleJump;
    }

    private void HandleTriggerChange (Collider2D other)
    {
        if (bottomTrigger.corpseTriggered || bottomTrigger.playerTriggered)
        {
            rb.mass = 0.001f;
        }
        else
        {
            rb.mass = defaultMass;
        }
    }

    private void HandleJump ()
    {
        rb.mass = defaultMass;
    }
}
