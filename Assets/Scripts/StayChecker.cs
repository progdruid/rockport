using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayChecker : MonoBehaviour
{
    public bool stayingOnGround { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        stayingOnGround = true;
        transform.parent.parent = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        stayingOnGround = false;
        transform.parent.parent = null;
    }
}
