using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishDoor : MonoBehaviour
{
    public int LoadLevelIndex;

    private void OnTriggerEnter2D(Collider2D col)
    {
        Registry.ins.lm.LoadLevel(LoadLevelIndex);
    }
}
