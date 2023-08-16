using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySoundPlayer : MonoBehaviour
{
    [SerializeField] float landingCooldown;
    [SerializeField] BodySideTrigger bottomTrigger;

    private CustomSoundEmitter soundEmitter;
    private Player player;

    private TriggeredType standingType;
    private float lastLandingTime;

    void Start()
    {
        soundEmitter = GetComponent<CustomSoundEmitter>();
        player = GetComponent<Player>();

        bottomTrigger.EnterEvent += HandleLanding;
        bottomTrigger.ExitEvent += HandleExit;
        player.PreJumpEvent += HandleJump;
    }

    void OnDestroy()
    {
        bottomTrigger.EnterEvent -= HandleLanding;
        bottomTrigger.ExitEvent -= HandleExit;
        player.PreJumpEvent -= HandleJump;
    }

    private void HandleLanding (Collider2D other, TriggeredType type)
    {
        bool valid = standingType == TriggeredType.None && (lastLandingTime == 0 || Time.time - lastLandingTime >= landingCooldown);

        if (valid && type == TriggeredType.Dirt)
            soundEmitter.EmitSound("LandingGrass");
        else if (valid)
            soundEmitter.EmitSound("LandingSolid");

        lastLandingTime = Time.time;
        standingType = type;
    }

    private void HandleExit (Collider2D other, TriggeredType type)
    {
        if (!bottomTrigger.triggered)
            standingType = TriggeredType.None;
    }

    private void HandleJump ()
    {
        soundEmitter.EmitSound("Jump");
    }

    private void HandleWalkingChange (TriggeredType type)
    {

    }
}
