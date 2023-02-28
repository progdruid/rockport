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
        bool isPlayer = pressingBody.TryGetComponent(out Player player);
        bool isCorpse = pressingBody.TryGetComponent(out CorpsePhysics corpse);
        Registry.ins.inputSystem.CanJump = !isPlayer;

        animator.SetTrigger("Pressed");

        yield return new WaitForSeconds(TimeOffset);


        //jump pads should always push on the same height, even if
        //there are multible bodies stacked one onto another
        float massMult = 1f;
        bool found = pressingBody.TryGetComponent(out MassDivider massDivider);
        if (found)
            massMult = massDivider.massMult;

        //player physics does not work well with horizontal jump pads, because of deceleration
        //therefore the greater multipilier is used
        float bodySpecificHorMult = 1f;
        if (isPlayer)
           bodySpecificHorMult = DefaultPlayerHorizontalMultiplier;

        //calc angle
        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        angle = angle > Mathf.PI ? -(Mathf.PI * 2 - angle) : angle;

        //calc the velocity xy
        float velX = pressingBody.velocity.x * Mathf.Cos(angle) - Impulse * massMult * Mathf.Sin(angle) * bodySpecificHorMult;
        float velY = pressingBody.velocity.y * Mathf.Sin(angle) + Impulse * massMult * Mathf.Cos(angle);
        
        pressingBody.velocity = new Vector2(velX, velY);


        //if it is a corpse, give a kicked mode to this corpse
        bool isInRange = (Mathf.Abs(angle) < Mathf.PI * 3f / 4f && Mathf.Abs(angle) > Mathf.PI / 4f);
        if (isCorpse && isInRange)
            corpse.kickedMode = true;

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
