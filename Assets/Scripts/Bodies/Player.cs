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

        Registry.ins.playerManager.PlayerDeathEvent += HandleDeath;

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
        
        ComputeHorizontalVelocity();

        animator.SetBool("Grounded", bottomTrigger.triggered);
        animator.SetBool("Running", rb.linearVelocity.x != 0f && bottomTrigger.triggered);
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
        if (rb.linearVelocity.y >= 0 && !ultraJumped)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * SuppressMultiplier);
    }

    private void ApplyVerticalVelocity(float initJumpVelocity)
    {
        float newVel = rb.linearVelocity.y + initJumpVelocity;
        newVel = Mathf.Clamp(newVel, -100, initJumpVelocity * MaxJumpImpulseMultiplier);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVel);
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
            float newvel = rb.linearVelocity.x + addvel;

            if (Mathf.Abs(newvel) > speed)
                newvel = speed * Mathf.Sign(newvel);

            rb.linearVelocity = new Vector2(newvel, rb.linearVelocity.y);
            if (leftTrigger.corpseTriggered && newvel < 0f)
                leftTrigger.body.linearVelocity = new Vector2(newvel * 0.9f, leftTrigger.body.linearVelocity.y);
            else if (rightTrigger.corpseTriggered && newvel > 0f)
                rightTrigger.body.linearVelocity = new Vector2(newvel * 0.9f, rightTrigger.body.linearVelocity.y);
        }
        else if (value == 0f && rb.linearVelocity.x != 0f) //deceleration
        {
            float sign = Mathf.Sign(rb.linearVelocity.x);
            float newvel = rb.linearVelocity.x - dec * Time.deltaTime * sign;
            if (newvel * sign < 0f)
                newvel = 0f;

            rb.linearVelocity = new Vector2(newvel, rb.linearVelocity.y);
        }
    }

    private void HandleDeath ()
    {
        gameObject.layer = 10;
        animator.SetBool("Died", true);
    }

    private void OnDestroy()
    {
        Registry.ins.inputSet.JumpKeyPressEvent -= MakeRegularJump;
        Registry.ins.inputSet.JumpKeyReleaseEvent -= SuppressJump;

        Registry.ins.playerManager.PlayerDeathEvent -= HandleDeath;
    }
}
