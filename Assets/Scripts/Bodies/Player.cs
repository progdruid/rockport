using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public fields
    public float MaxSpeed;
    public float PushingSpeed;
    public float AccTime;
    public float DecTime;
    public float RiseTime;
    public float JumpHeight;
    public float CoyoteTime;
    public float JumpCooldown;
    public float SuppressMultiplier;
    public float MaxJumpImpulseMultiplier;
    [Space]
    public BodySideTrigger rightTrigger;
    public BodySideTrigger leftTrigger;

    [HideInInspector] public bool pushedByPad;

    //classes
    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private StayChecker stayChecker;

    //private fields
    private float coyoteTime = 0f;
    private float jumpCooldown = 0f;

    private float speed;
    private float acc;
    private float dec;
    private float jumpImpulse; //ok, nerd. it's not an impulse. it's a starting velocity 

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.TryGetComponent(out stayChecker);
            if (stayChecker != null)
                break;
        }

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<Collider2D>();

        Registry.ins.inputSystem.JumpKeyPressEvent += Jump;
        Registry.ins.inputSystem.JumpKeyReleaseEvent += SuppressJump;

        InitValues();
    }

    private void InitValues ()
    {
        speed = MaxSpeed;
        acc = MaxSpeed / AccTime;
        dec = MaxSpeed / DecTime;
        jumpImpulse = JumpHeight * 2f / RiseTime;
        rb.gravityScale = jumpImpulse / (RiseTime * 9.8f);
    }

    void Update()
    {
        speed = (leftTrigger.corpseTriggered || rightTrigger.corpseTriggered) ? PushingSpeed : MaxSpeed;
        coyoteTime = stayChecker.stayingOnGround ? 0f : coyoteTime + Time.deltaTime;
        jumpCooldown += jumpCooldown < JumpCooldown ? Time.deltaTime : 0f;
        pushedByPad = !stayChecker.stayingOnGround && pushedByPad;

        MoveSide();

        animator.SetBool("Falling", !stayChecker.stayingOnGround);
        animator.SetBool("Running", rb.velocity.x != 0f && stayChecker.stayingOnGround);
    }

    public void ResetJumpCooldown () => jumpCooldown = 0f;

    private void Jump ()
    {
        if (coyoteTime <= CoyoteTime && jumpCooldown >= JumpCooldown && !pushedByPad)
        {
            float newVel = rb.velocity.y + jumpImpulse;
            newVel = Mathf.Clamp(newVel, -100, jumpImpulse * MaxJumpImpulseMultiplier);
            rb.velocity = new Vector2(rb.velocity.x, newVel);
            ResetJumpCooldown();
        }
    }

    private void SuppressJump ()
    {
        if (rb.velocity.y >= 0 && !pushedByPad)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * SuppressMultiplier);
    }

    private void MoveSide ()
    {
        float value = Registry.ins.inputSystem.HorizontalValue;
        spriteRenderer.flipX = value != 0f ? value < 0f : spriteRenderer.flipX;

        bool wallFree = !(leftTrigger.wallTriggered && value < 0f) && !(rightTrigger.wallTriggered && value > 0f);

        if (value != 0f)  //acceleration
        {
            float addvel = acc * Time.deltaTime * value * (wallFree ? 1f : 0.1f);
            float newvel = rb.velocity.x + addvel;

            if (Mathf.Abs(newvel) > speed)
                newvel = speed * Mathf.Sign(newvel);

            rb.velocity = new Vector2(newvel, rb.velocity.y);
            if (leftTrigger.corpseTriggered && newvel < 0f)
                leftTrigger.corpseRB.velocity = new Vector2(newvel * 0.9f, leftTrigger.corpseRB.velocity.y);
            else if (rightTrigger.corpseTriggered && newvel > 0f)
                rightTrigger.corpseRB.velocity = new Vector2(newvel * 0.9f, rightTrigger.corpseRB.velocity.y);
        }
        else if (value == 0f && rb.velocity.x != 0f) //deceleration
        {
            float sign = Mathf.Sign(rb.velocity.x);
            float newvel = rb.velocity.x - dec * Time.deltaTime * sign;
            if (newvel * sign < 0f)
                newvel = 0f;

            rb.velocity = new Vector2(newvel, rb.velocity.y);
        }
    }

    public void PlayDeathAnimation ()
    {
        animator.SetBool("Died", true);
    }

    private void OnDestroy()
    {
        Registry.ins.inputSystem.JumpKeyPressEvent -= Jump;
        Registry.ins.inputSystem.JumpKeyReleaseEvent -= SuppressJump;
    }
}
