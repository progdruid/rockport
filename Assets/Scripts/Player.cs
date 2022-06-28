using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public fields
    public float MaxSpeed;
    public float AccTime;
    public float DecTime;
    [Space]
    public float JumpForce;
    [Space]
    public float BBound;
    public float FallingTimeThreshold;
    [Space]
    public bool active = false;

    //classes
    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private GameManager gameManager;

    //private fields
    private float fallingTime = 0f;
    private bool touchesLeft = false;
    private bool touchesRight = false;

    private float acc => MaxSpeed / AccTime;
    private float dec => MaxSpeed / DecTime;

    //reserved list
    private List<ContactPoint2D> contactPoints;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<Collider2D>();

        contactPoints = new List<ContactPoint2D>();
    }

    void Update()
    {
        collider.GetContacts(contactPoints);
        CheckForCollision(contactPoints);

        if (active)
        {
            MoveSide();

            if (Input.GetKeyDown(KeyCode.Space) && fallingTime < FallingTimeThreshold) //Jump //Here is an exploit
            {
                rb.velocity += Vector2.up * JumpForce;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                gameManager.KillPlayer();
            }

            if (Input.GetKey(KeyCode.Q) && Input.GetMouseButtonDown(0)) //this one will be an easter egg
            {
                gameManager.RevokeCorpseAt(transform.GetChild(0).GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition));
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                gameManager.RevokeFirstCorpse();
            }
        }
        animator.SetBool("Falling", fallingTime >= FallingTimeThreshold);
        animator.SetBool("Running", rb.velocity.x != 0f && fallingTime < FallingTimeThreshold);
    }

    private void MoveSide ()
    {
        float value = Input.GetAxisRaw("Horizontal");
        spriteRenderer.flipX = value != 0f ? value < 0f : spriteRenderer.flipX;

        bool wallFree = !(touchesLeft && value < 0f) && !(touchesRight && value > 0f);

        if (value != 0f)  //acceleration
        {
            float newvel = rb.velocity.x + acc * Time.deltaTime * value * (wallFree ? 1f : 0.5f);

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
        bool oneTouchesLeft = false;
        bool oneTouchesRight = false;
        bool falls = true;

        foreach (var contact in contacts)
        {
            Vector2 point = contact.point - (Vector2)gameObject.transform.position;
            point.Normalize();

            if (point.y <= -BBound) //Checking bottom
            {
                falls = false;
                fallingTime = 0f;
            }

            if (!oneTouchesLeft && point.x < 0f)                //Checking left side
                oneTouchesLeft = Mathf.Abs(point.y) < BBound;
            else if (!oneTouchesRight && point.x > 0f)          //Checking right side
                oneTouchesRight = Mathf.Abs(point.y) < BBound;
        }
        if (falls)
            fallingTime += Time.deltaTime;

        touchesLeft = oneTouchesLeft;
        touchesRight = oneTouchesRight;
    }

    public void PlayDeathAnimation ()
    {
        animator.SetBool("Died", true);
    }
}
