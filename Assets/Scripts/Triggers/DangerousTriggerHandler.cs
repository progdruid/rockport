using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousTriggerHandler : MonoBehaviour
{
    public void OnTriggerEnter2D (Collider2D col)
    {
        if (col.gameObject.layer == 7)
            return;

        if (!col.gameObject.GetComponent<SignComponent>().HasSign("Player") || !Registry.ins.inputSystem.GetActive())
            return;

        Registry.ins.playerManager.KillPlayer();
    }
}
