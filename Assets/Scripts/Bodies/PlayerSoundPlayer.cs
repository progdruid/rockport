using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundPlayer : BodySoundPlayer
{
    private Player player;

    protected override void Start()
    {
        base.Start();

        player = GetComponent<Player>();

        player.PreJumpEvent += HandleJump;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        player.PreJumpEvent -= HandleJump;
    }

    private void HandleJump()
    {
        soundEmitter.EmitSound("Jump");
    }

    private void HandleWalkingChange(TriggeredType type)
    {

    }
}
