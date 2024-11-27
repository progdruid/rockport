using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpsePhysics : MonoBehaviour, IUltraJumper
{
    [SerializeField] float drag;
    [SerializeField] BodySideTrigger bottomTrigger;
    [SerializeField] float UltraJumpCooldown;
    [SerializeField] float MaxJumpImpulseMultiplier;

    private Rigidbody2D rb;

    private bool ultraJumped = false;
    private float ultraJumpCooldown = 0f;

    public bool kickedMode;

    private void Start()
    {
        //stayChecker = transform.GetChild(0).GetComponent<StayChecker>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ultraJumped = !bottomTrigger.triggered && ultraJumped;
        ultraJumpCooldown += ultraJumpCooldown < UltraJumpCooldown ? Time.deltaTime : 0f;

        if (!kickedMode)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * (1f - drag * Time.deltaTime), rb.linearVelocity.y);
        else
            kickedMode = !bottomTrigger.triggered;
    }

    public void PresetUltraJumped(bool value) => ultraJumped = value;

    public void MakeUltraJump(float initJumpVelocity)
    {
        if (ultraJumpCooldown >= UltraJumpCooldown)
        {
            ultraJumped = true;
            ultraJumpCooldown = 0f;

            float newVel = rb.linearVelocity.y + initJumpVelocity;
            newVel = Mathf.Clamp(newVel, -100, initJumpVelocity * MaxJumpImpulseMultiplier);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVel);
        }
    }
}
