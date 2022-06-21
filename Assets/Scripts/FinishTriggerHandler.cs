using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    public int LoadLevelIndex;

    GameLoader loader;

    private void Start()
    {
        loader = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameLoader>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        loader.LoadLevel(LoadLevelIndex);
    }
}
