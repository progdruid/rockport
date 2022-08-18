using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float Impulse;
    public float TimeOffset;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        string tag = col.gameObject.tag;
        if (tag != "Player" && tag != "Corpse")
            return;

        StartCoroutine(Push(col.gameObject.GetComponent<Rigidbody2D>()));

    }

    private IEnumerator Push (Rigidbody2D pressingBody)
    {
        animator.SetTrigger("Pressed");
        yield return new WaitForSeconds(TimeOffset);
        pressingBody.velocity = new Vector2(pressingBody.velocity.x, Impulse);
    }
}
