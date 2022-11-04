using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayChecker : MonoBehaviour
{
    public bool stayingOnGround { get; private set; }

    public float additionalMass { get; set; }
    public float normalMass { get; private set; }
    public float massMult => (normalMass + additionalMass) / normalMass;

    public event System.Action<Collider2D> EnterEvent = delegate { };
    public event System.Action<Collider2D> StayEvent = delegate { };
    public event System.Action<Collider2D> ExitEvent = delegate { };

    private StayChecker otherChecker;

    private void Start()
    {
        var rb = transform.parent.GetComponent<Rigidbody2D>();
        normalMass = rb.mass;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnterEvent(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        stayingOnGround = true;
        transform.parent.parent = other.transform;

        if (otherChecker != null)
            return;

        bool found = false;
        if (other.transform.childCount > 0)
            found = other.transform.GetChild(0).TryGetComponent(out otherChecker);

        if (found)
            otherChecker.additionalMass += normalMass + additionalMass;

        StayEvent(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        stayingOnGround = false;
        transform.parent.parent = null;
        if (otherChecker != null)
            otherChecker.additionalMass -= normalMass + additionalMass;
        otherChecker = null;

        ExitEvent(other);
    }
}
