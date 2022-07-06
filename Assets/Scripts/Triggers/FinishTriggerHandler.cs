using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    public int LoadLevelIndex;

    GameManager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        manager.LoadLevel(LoadLevelIndex);
    }
}
