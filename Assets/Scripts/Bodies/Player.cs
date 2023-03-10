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

    private Rigidbody2D pushingRB;
    private bool pushingLeft = false;
    private bool pushingRight = false;
    private bool wallLeft = false;
    private bool wallRight = false;
    //private bool staysOnGround = false;

    private float speed;
    private float acc;
    private float dec;
    private float jumpImpulse; //ok, nerd. it's not an impulse. it's a starting velocity 

    //reserved list
    private List<ContactPoint2D> contactPoints;

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

        contactPoints = new List<ContactPoint2D>();

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
        collider.GetContacts(contactPoints);
        CheckForCollision(contactPoints);

        speed = (pushingLeft || pushingRight) ? PushingSpeed : MaxSpeed;
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

        bool wallFree = !(wallLeft && value < 0f) && !(wallRight && value > 0f);

        if (value != 0f)  //acceleration
        {
            float addvel = acc * Time.deltaTime * value * (wallFree ? 1f : 0.1f);
            float newvel = rb.velocity.x + addvel;

            if (Mathf.Abs(newvel) > speed)
                newvel = speed * Mathf.Sign(newvel);

            rb.velocity = new Vector2(newvel, rb.velocity.y);
            if((pushingLeft && newvel < 0f) || (pushingRight && newvel > 0f))
                pushingRB.velocity = new Vector2(newvel * 0.9f, pushingRB.velocity.y);
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

    private void CheckForCollision (List<ContactPoint2D> contacts)
    {
        wallLeft = false;
        wallRight = false; 
        //staysOnGround = false;
        pushingLeft = false;
        pushingRight = false;
        pushingRB = null;

        foreach (var contact in contacts)
        {
            Vector2 point = contact.point - (Vector2)gameObject.transform.position;
            point.Normalize();

            //staysOnGround = Mathf.Abs(point.x) < -point.y || staysOnGround;

            bool touchesBody = contact.collider.TryGetComponent(out SignComponent sign) && sign.HasSign("Body");
            bool touchesSide = Mathf.Abs(point.y) < Mathf.Abs(point.x);
            bool touchesLeft = touchesSide && point.x < 0;
            bool touchesRight = touchesSide && point.x > 0;
            pushingLeft = (touchesBody && touchesLeft) || pushingLeft;
            pushingRight = (touchesBody && touchesRight) || pushingRight;
            pushingRB = (touchesBody && touchesSide) ? sign.GetComponent<Rigidbody2D>() : pushingRB;
            wallLeft = (touchesSide && point.x < 0f && !pushingLeft) || wallLeft;
            wallRight = (touchesSide && point.x > 0f && !pushingRight) || wallRight;
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
