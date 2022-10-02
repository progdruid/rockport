using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    public int LoadLevelIndex;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 7)
            return;

        Registry.ins.lm.LoadLevel(LoadLevelIndex);
    }
}
