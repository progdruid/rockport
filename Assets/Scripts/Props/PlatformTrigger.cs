using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public Collider2D platformCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Physics2D.IgnoreCollision(other, platformCollider, false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Physics2D.IgnoreCollision(other, platformCollider, true);
    }
}
