using UnityEngine;

public class StayChecker : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {    
        transform.parent.parent = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        transform.parent.parent = null;
    }
}
