using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public fields
    public float MaxSpeed;
    public float AccTime;
    public float DecTime;
    public float RiseTime;
    public float JumpHeight;
    public float CoyoteTime;
    public float SuppressMultiplier;

    //classes
    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private StayChecker stayChecker;

    //private fields
    private float coyoteTime = 0f;

    private bool pushing = false;
    private bool wallLeft = false;
    private bool wallRight = false;
    private bool staysOnGround = false;

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
#if UNITY_EDITOR
    private void OnValidate()
    {   
        if (Application.isPlaying)
            InitValues();
    }
#endif

    private void InitValues ()
    {
        acc = MaxSpeed / AccTime;
        dec = MaxSpeed / DecTime;
        jumpImpulse = JumpHeight * 2f / RiseTime;
        rb.gravityScale = jumpImpulse / (RiseTime * 9.8f);
    }

    void Update()
    {
        collider.GetContacts(contactPoints);
        CheckForCollision(contactPoints);

        if (staysOnGround)
        {
            coyoteTime = 0f;
        }
        else
        {
            coyoteTime += Time.deltaTime;
        }

        MoveSide();
        Debug.Log(pushing);
        animator.SetBool("Falling", !stayChecker.stayingOnGround);
        animator.SetBool("Running", rb.velocity.x != 0f && stayChecker.stayingOnGround);
    }

    private void Jump ()
    {
        if (coyoteTime <= CoyoteTime)
            rb.velocity += Vector2.up * jumpImpulse;
    }

    private void SuppressJump ()
    {
        if (rb.velocity.y >= 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * SuppressMultiplier);
    }

    private void MoveSide ()
    {
        float value = Registry.ins.inputSystem.HorizontalValue;
        spriteRenderer.flipX = value != 0f ? value < 0f : spriteRenderer.flipX;

        bool wallFree = !(wallLeft && value < 0f) && !(wallRight && value > 0f);

        if (value != 0f)  //acceleration
        {
            float newvel = rb.velocity.x + acc * Time.deltaTime * value * (wallFree ? 1f : 0.1f);

            if (Mathf.Abs(newvel) > MaxSpeed)
                newvel = MaxSpeed * Mathf.Sign(newvel);

            rb.velocity = new Vector2(newvel, rb.velocity.y);
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

    public void CheckForCollision (List<ContactPoint2D> contacts)
    {
        wallLeft = false;
        wallRight = false; 
        staysOnGround = false;
        pushing = false;

        foreach (var contact in contacts)
        {
            Vector2 point = contact.point - (Vector2)gameObject.transform.position;
            point.Normalize();

            staysOnGround = Mathf.Abs(point.x) < -point.y || staysOnGround;

            bool touchesBody = contact.collider.TryGetComponent(out SignComponent sign) && sign.HasSign("Body");
            bool touchesSide = Mathf.Abs(point.y) < Mathf.Abs(point.x);
            pushing = (touchesBody && touchesSide) || pushing;
            wallLeft = (touchesSide && point.x < 0f && !pushing) || wallLeft;
            wallRight = (touchesSide && point.x > 0f && !pushing) || wallRight;
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
