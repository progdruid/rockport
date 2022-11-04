using UnityEngine;

public class StayChecker : MonoBehaviour
{
    public bool stayingOnGround { get; private set; }

    public event System.Action<Collider2D> EnterEvent = delegate { };
    public event System.Action<Collider2D> StayEvent = delegate { };
    public event System.Action<Collider2D> ExitEvent = delegate { };

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnterEvent(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        stayingOnGround = true;
        transform.parent.parent = other.transform;

        StayEvent(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        stayingOnGround = false;
        transform.parent.parent = null;

        ExitEvent(other);
    }
}
