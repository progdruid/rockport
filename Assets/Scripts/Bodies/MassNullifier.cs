using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassNullifier : MonoBehaviour
{
    //[SerializeField] StayChecker stayChecker;
    [SerializeField] BodySideTrigger bottomTrigger;
    [SerializeField] Rigidbody2D rb;

    private float defaultMass;

    void Start()
    {
        defaultMass = rb.mass;

        bottomTrigger.EnterEvent += HandleTriggerChange;
        bottomTrigger.ExitEvent += HandleTriggerChange;
    }

    private void OnDestroy()
    {
        bottomTrigger.EnterEvent -= HandleTriggerChange;
        bottomTrigger.ExitEvent -= HandleTriggerChange;
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


}
