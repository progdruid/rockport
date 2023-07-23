using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IUltraJumper
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
    public float UltraJumpCooldown;
    public float SuppressMultiplier;
    public float MaxJumpImpulseMultiplier;
    public float AnimationJumpDisableCooldown;
    [Space]
    public BodySideTrigger rightTrigger;
    public BodySideTrigger leftTrigger;
    public BodySideTrigger bottomTrigger;

    public event System.Action PreJumpEvent = delegate { };

    //classes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    //private fields
    private bool ultraJumped = false;

    private float coyoteTime = 0f;
    private float jumpCooldownTime = 0f;
    private float ultraJumpCooldownTime = 0f;
    private float animJumpDisableCDTime = 0f;

    private float speed;
    private float acc;
    private float dec;
    private float jumpImpulse; //ok, nerd. it's not an impulse. it's a starting velocity 

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        Registry.ins.inputSet.JumpKeyPressEvent += MakeRegularJump;
        Registry.ins.inputSet.JumpKeyReleaseEvent += SuppressJump;

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
        coyoteTime = bottomTrigger.triggered ? 0f : coyoteTime + Time.deltaTime;
        jumpCooldownTime += jumpCooldownTime < JumpCooldown ? Time.deltaTime : 0f;
        ultraJumpCooldownTime += ultraJumpCooldownTime < UltraJumpCooldown ? Time.deltaTime : 0f;
        animJumpDisableCDTime += animJumpDisableCDTime < AnimationJumpDisableCooldown ? Time.deltaTime : 0f;
        ultraJumped = !bottomTrigger.triggered && ultraJumped;
        //Debug.Log(leftTrigger.triggered);
        ComputeHorizontalVelocity();

        animator.SetBool("Grounded", bottomTrigger.triggered);
        animator.SetBool("Running", rb.velocity.x != 0f && bottomTrigger.triggered);
        if (animJumpDisableCDTime >= AnimationJumpDisableCooldown && animator.GetBool("Jumped"))
            animator.SetBool("Jumped", !bottomTrigger.triggered);
    }

    public void ResetJumpCooldown () => jumpCooldownTime = 0f;

    public void PresetUltraJumped(bool value) => ultraJumped = value;

    public void MakeUltraJump(float initJumpVelocity)
    {
        if (ultraJumpCooldownTime >= UltraJumpCooldown)
        {
            ultraJumped = true;
            ultraJumpCooldownTime = 0f;
            ApplyVerticalVelocity(initJumpVelocity);
        }
    }

    private void MakeRegularJump()
    {
        if (coyoteTime <= CoyoteTime && jumpCooldownTime >= JumpCooldown && !ultraJumped)
        {
            PreJumpEvent();
            ApplyVerticalVelocity(jumpImpulse);
            ResetJumpCooldown();
        }
    }

    private void SuppressJump ()
    {
        if (rb.velocity.y >= 0 && !ultraJumped)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * SuppressMultiplier);
    }

    private void ApplyVerticalVelocity(float initJumpVelocity)
    {
        float newVel = rb.velocity.y + initJumpVelocity;
        newVel = Mathf.Clamp(newVel, -100, initJumpVelocity * MaxJumpImpulseMultiplier);
        rb.velocity = new Vector2(rb.velocity.x, newVel);
        animator.SetBool("Jumped", true);
        animJumpDisableCDTime = 0f;
    }

    private void ComputeHorizontalVelocity ()
    {
        float value = Registry.ins.inputSet.HorizontalValue;
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
        gameObject.layer = 10;
        animator.SetBool("Died", true);
    }

    private void OnDestroy()
    {
        Registry.ins.inputSet.JumpKeyPressEvent -= MakeRegularJump;
        Registry.ins.inputSet.JumpKeyReleaseEvent -= SuppressJump;
    }
}
