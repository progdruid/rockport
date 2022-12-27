using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerObject : MonoBehaviour
{
    public void OnTriggerEnter2D (Collider2D col)
    {
        bool isSigned = col.gameObject.TryGetComponent(out SignComponent sign);
        if (!isSigned || !sign.HasSign("Player") || !Registry.ins.inputSystem.Active)
            return;

        Registry.ins.playerManager.KillPlayer();
    }
}
