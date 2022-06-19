using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float MaxSpeed;
    public float AccTime;
    public float DecTime;
    [Space]
    public float JumpForce;
    public float JumpCooldown;
    [Space]
    public float BBound;

    private float acc => MaxSpeed / AccTime;
    private float dec => MaxSpeed / DecTime;

    private Rigidbody2D rb;
    private CircleCollider2D collider;
    private bool staying = false;
    private bool touchesLeft = false;
    private bool touchesRight = false;

    private List<ContactPoint2D> contactPoints;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<CircleCollider2D>();
        contactPoints = new List<ContactPoint2D>();
    }

    void Update()
    {
        collider.GetContacts(contactPoints);
        CheckForCollision(contactPoints);

        float horvalue = Input.GetAxisRaw("Horizontal");
        bool wallFree = !(touchesLeft && horvalue < 0f) && !(touchesRight && horvalue > 0f);


        if (horvalue != 0f && wallFree)  //acceleration
        {
            float newvel = rb.velocity.x + acc * Time.deltaTime * horvalue;

            if (Mathf.Abs(newvel) > MaxSpeed)
                newvel = MaxSpeed * Mathf.Sign(newvel);

            rb.velocity = new Vector2(newvel, rb.velocity.y);
        }
        else if (horvalue == 0f && rb.velocity.x != 0f) //deceleration
        {
            float sign = Mathf.Sign(rb.velocity.x);
            float newvel = rb.velocity.x - dec * Time.deltaTime * sign;
            if (newvel * sign < 0f)
                newvel = 0f;

            rb.velocity = new Vector2(newvel, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.Space) && staying) //Jump
            rb.velocity += Vector2.up * JumpForce;

        
    }

    public void CheckForCollision (List<ContactPoint2D> contacts)
    {
        bool oneTouchesLeft = false;
        bool oneTouchesRight = false;
        staying = false;

        foreach (var contact in contacts)
        {
            Vector2 point = contact.point - (Vector2)gameObject.transform.position;
            point.Normalize();

            if (point.y <= -BBound && !staying) //Checking bottom
                staying = true;

            if (!oneTouchesLeft && point.x < 0f)                //Checking left side
                oneTouchesLeft = Mathf.Abs(point.y) < BBound;
            else if (!oneTouchesRight && point.x > 0f)          //Checking right side
                oneTouchesRight = Mathf.Abs(point.y) < BBound;
        }

        touchesLeft = oneTouchesLeft;
        touchesRight = oneTouchesRight;
    }
}
