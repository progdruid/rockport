using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpsePhysics : MonoBehaviour
{
    [SerializeField] float drag;

    private StayChecker stayChecker;
    private Rigidbody2D rb;

    public bool kickedMode;

    private void Start()
    {
        stayChecker = transform.GetChild(0).GetComponent<StayChecker>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!kickedMode)
            rb.velocity = new Vector2(rb.velocity.x * (1f - drag * Time.deltaTime), rb.velocity.y);
        else
            kickedMode = !stayChecker.stayingOnGround;
    }
}
