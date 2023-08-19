using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseSoundController : BodySoundController
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
