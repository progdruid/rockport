using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private Animator animator;
    private bool activated;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 7)
            return;

        if (!col.gameObject.GetComponent<SignComponent>().HasSign("Player") || activated)
            return;

        activated = true;
        Registry.ins.playerManager.SetSpawnPoint(transform.position);
        animator.SetTrigger("Burned");
    }
}
