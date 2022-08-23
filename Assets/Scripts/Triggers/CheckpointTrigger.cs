using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private LevelManager gm;
    private Animator animator;
    private bool activated;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelManager>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag != "Player" || activated)
            return;

        activated = true;
        gm.respawnPoint = transform.position;
        animator.SetTrigger("Burned");
    }
}
