using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayChecker : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        transform.parent.parent = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        transform.parent.parent = null;
    }
}
