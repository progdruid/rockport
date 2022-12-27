using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator animator;
    private bool activated;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.GetComponent<SignComponent>().HasSign("Body") || activated)
            return;

        activated = true;
        Registry.ins.playerManager.SetSpawnPoint(transform.position);
        animator.SetTrigger("Burned");
    }
}
