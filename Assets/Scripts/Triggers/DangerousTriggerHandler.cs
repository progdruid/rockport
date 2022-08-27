using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousTriggerHandler : MonoBehaviour
{
    private LevelManager gameManager;

    private void Start()
    {
        gameManager = SignComponent.FindEntity("LevelManager").GetComponent<LevelManager>();
    }

    public void OnTriggerEnter2D (Collider2D col)
    {
        if (col.gameObject.layer == 7)
            return;

        if (!col.gameObject.GetComponent<SignComponent>().HasSign("Player") || !InputSystem.ins.GetActive())
            return;

        gameManager.KillPlayer();
    }
}
