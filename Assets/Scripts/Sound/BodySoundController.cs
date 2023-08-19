using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySoundController : MonoBehaviour
{
    [SerializeField] protected float landingCooldown;
    [SerializeField] protected BodySideTrigger bottomTrigger;
    [SerializeField] protected CustomSoundEmitter soundEmitter;
    
    protected Rigidbody2D rb;

    protected TriggeredType standingType;
    protected float lastLandingTime;

    protected virtual void Start()
    {
        //soundEmitter = GetComponent<CustomSoundEmitter>();
        rb = GetComponent<Rigidbody2D>();

        bottomTrigger.EnterEvent += HandleLanding;
        bottomTrigger.ExitEvent += HandleExit;
    }

    protected virtual void OnDestroy()
    {
        bottomTrigger.EnterEvent -= HandleLanding;
        bottomTrigger.ExitEvent -= HandleExit;
    }

    protected virtual void HandleLanding (Collider2D other, TriggeredType type)
    {
        bool valid = standingType == TriggeredType.None && (lastLandingTime == 0 || Time.time - lastLandingTime >= landingCooldown) && rb.velocity.y <= 0;

        if (valid && type == TriggeredType.Dirt)
            soundEmitter.EmitSound("LandingGrass");
        else if (valid)
            soundEmitter.EmitSound("LandingSolid");

        lastLandingTime = Time.time;
        standingType = type;
    }

    protected virtual void HandleExit (Collider2D other, TriggeredType type)
    {
        if (!bottomTrigger.triggered)
            standingType = TriggeredType.None;
    }
}
