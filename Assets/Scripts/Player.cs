using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float SpeedOnGround;
    public float SpeedInAir;
    public float JumpForce;
    [Space]
    public float BBound;
    public float TimeForMovement;
    public float TimeForJump;

    private Rigidbody2D rb;
    private float timeStaying = 0f;
    private bool touchesLeft = false;
    private bool touchesRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float vel = Input.GetAxis("Horizontal");

        if (timeStaying > TimeForMovement)
            vel *= SpeedOnGround;
        else
            vel *= SpeedInAir;

        if (vel != 0f && !(touchesLeft && vel < 0f) && !(touchesRight && vel > 0f))
            rb.velocity = new Vector2(vel, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && timeStaying >= TimeForJump)
        {
            rb.velocity += Vector2.up * JumpForce;
            timeStaying = 0f;
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Ground")
            return;

        bool oneTouchesLeft = false;
        bool oneTouchesRight = false;

        bool groundChecked = false;

        foreach (var contact in collision.contacts)
        {
            Vector2 point = contact.point - (Vector2)gameObject.transform.position;
            point.Normalize();

            if (point.y <= -BBound && timeStaying < 1 && !groundChecked) //Checking bottom
            {
                timeStaying += Time.deltaTime;
                groundChecked = true;
            }
            else if (!oneTouchesLeft && point.x < 0f)           //Checking left side
                oneTouchesLeft = Mathf.Abs(point.y) < BBound;
            else if (!oneTouchesRight && point.x > 0f)          //Checking right side
                oneTouchesRight = Mathf.Abs(point.y) < BBound;
        }

        touchesLeft = oneTouchesLeft;
        touchesRight = oneTouchesRight;
    }
}
