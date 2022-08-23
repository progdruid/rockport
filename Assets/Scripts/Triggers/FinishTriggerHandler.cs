using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    public int LoadLevelIndex;

    LevelManager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        manager.LoadLevel(LoadLevelIndex);
    }
}
