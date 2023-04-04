using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float Impulse;
    public float TimeOffset;
    public float DefaultPlayerHorizontalMultiplier;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool found = other.gameObject.TryGetComponent(out SignComponent sign);
        if (!found || !sign.HasSign("Body"))
            return;

        StartCoroutine(Push(other.gameObject.GetComponent<Rigidbody2D>(), sign));
    }

    private IEnumerator Push (Rigidbody2D pressingBody, SignComponent sign)
    {
        //disables jump if it's a trigPlayer //lil hack to avoid the if statement
        bool isPlayer = pressingBody.TryGetComponent(out Player player);
        Registry.ins.inputSystem.CanJump = !isPlayer;

        animator.SetTrigger("Pressed");

        yield return new WaitForSeconds(TimeOffset);

        pressingBody.velocity += new Vector2(0f, Impulse);
        if (isPlayer)
            Registry.ins.playerManager.ResetJumpCooldown();
    }

    private void OnTriggerExit2D (Collider2D other)
    {
        bool found = other.TryGetComponent(out Player player);

        if (found)
        {
            Registry.ins.inputSystem.CanJump = true;
            player.pushedByPad = true;
        }
    }
}
