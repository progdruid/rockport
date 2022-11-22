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
        //disables jump if it's a player //lil hack to avoid the if statement
        bool isPlayer = sign.HasSign("Player");
        Registry.ins.inputSystem.CanJump = !isPlayer;

        animator.SetTrigger("Pressed");
        yield return new WaitForSeconds(TimeOffset);
        float massMult = 1f;
        bool found = pressingBody.TryGetComponent(out MassDivider massDivider);
        if (found)
            massMult = massDivider.massMult;

        float bodySpecificHorMult = 1f;
        if (isPlayer)
            bodySpecificHorMult = DefaultPlayerHorizontalMultiplier;

        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        angle = angle > Mathf.PI ? -(Mathf.PI * 2 - angle) : angle;
        float velX = pressingBody.velocity.x * Mathf.Cos(angle) - Impulse * massMult * Mathf.Sin(angle) * bodySpecificHorMult;
        float velY = pressingBody.velocity.y * Mathf.Sin(angle) + Impulse * massMult * Mathf.Cos(angle);
        pressingBody.velocity = new Vector2(velX, velY);
        //Debug.Log(angle);
        bool isInRange = (Mathf.Abs(angle) < Mathf.PI * 3f / 4f && Mathf.Abs(angle) > Mathf.PI / 4f);
        if (pressingBody.TryGetComponent(out CorpsePhysics corpse) && isInRange)
            corpse.kickedMode = true;

        if (isPlayer)
            Registry.ins.playerManager.ResetJumpCooldown();
    }

    private void OnTriggerExit2D (Collider2D other)
    {
        bool found = other.TryGetComponent(out SignComponent sign);

        //enables jump if it's a player //lil hack to avoid the if statement
        Registry.ins.inputSystem.CanJump = Registry.ins.inputSystem.CanJump || (found && sign.HasSign("Player"));
    }
}
