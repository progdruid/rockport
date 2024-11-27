using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : BodySoundController
{
    [SerializeField] PermutableSoundPlayer soundPlayer;
    
    private Player player;
    private bool wasRunningLastFrame = false;

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

    private void Update()
    {
        bool isRunning = bottomTrigger.triggered && rb.linearVelocity.x != 0;
        if (isRunning && !wasRunningLastFrame)
            soundPlayer.PlayAll();
        else if (!isRunning && wasRunningLastFrame)
            soundPlayer.Stop();
        wasRunningLastFrame = isRunning;

        if (bottomTrigger.dirtTriggered)
            soundPlayer.SelectClip("WalkGrass");
        else if (bottomTrigger.triggered)
            soundPlayer.SelectClip("WalkSolid");
        else
            soundPlayer.UnselectClip();

    }
}
