using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUltraJumper
{
    public void PresetUltraJumped(bool value);
    public void MakeUltraJump(float initJumpVelocity);
}
