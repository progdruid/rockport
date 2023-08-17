using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseSoundPlayer : BodySoundPlayer
{
    protected override void Start ()
    {
        base.Start ();

        HandleDeath ();
    }

    private void HandleDeath ()
    {
        soundEmitter.EmitSound("Death");
    }
}
