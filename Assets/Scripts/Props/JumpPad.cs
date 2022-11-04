using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        bool found = col.gameObject.TryGetComponent(out SignComponent sign);
        if (!found || !sign.HasSign("Body"))
            return;

        StartCoroutine(Push(col.gameObject.GetComponent<Rigidbody2D>()));

    }

    private IEnumerator Push (Rigidbody2D pressingBody)
    {
        animator.SetTrigger("Pressed");
        yield return new WaitForSeconds(TimeOffset);
        float massMult = 1f;
        bool found = pressingBody.TryGetComponent(out MassDivider massDivider);
        if (found)
            massMult = massDivider.massMult;
        
        pressingBody.velocity = new Vector2(pressingBody.velocity.x, Impulse * massMult);
        
    }
}
