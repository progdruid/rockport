using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    public int LoadLevelIndex;

    LevelManager manager;

    private void Start()
    {
        manager = SignComponent.FindEntity("LevelManager").GetComponent<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 7)
            return;

        manager.LoadLevel(LoadLevelIndex);
    }
}
