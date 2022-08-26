using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayChecker : MonoBehaviour
{
    public float colliderCenterHeight;
    public float minColliderX;
    public float maxColliderX;

    private Collider2D collider2D;
    private List<ContactPoint2D> contacts;

    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
        contacts = new List<ContactPoint2D>();

        if (!TryGetComponent(out BoxCollider2D collider))
            return;

        colliderCenterHeight = collider.offset.y;
        minColliderX = collider.offset.x - collider.size.x / 2f;
        maxColliderX = collider.offset.x + collider.size.x / 2f;
    }

    private bool IsValid (Vector2 relativeContactPoint)
    {
        return minColliderX <= relativeContactPoint.x && relativeContactPoint.x <= maxColliderX && relativeContactPoint.y >= colliderCenterHeight;
    }

    private void Update()
    {
        collider2D.GetContacts(contacts);
        SetParents(contacts);
    }

    private void SetParents (List<ContactPoint2D> contacts)
    {
        foreach (var contact in contacts)
        {
            if (contact.collider.gameObject.tag != "Player" && contact.collider.gameObject.tag != "Corpse")
                continue;

            Vector2 relativePoint = contact.point - (Vector2)transform.position;

            bool valid = IsValid(relativePoint);

            if (valid && contact.collider.transform.parent == null)
                contact.collider.transform.parent = transform;
        }
    }
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "Corpse")
            return;

        Vector2 averagePoint = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            averagePoint += collision.contacts[i].point;
        }
        averagePoint /= collision.contactCount;
        Vector2 relativePoint = averagePoint - (Vector2)transform.position;

        bool valid = IsValid(relativePoint);

        if (valid && collision.transform.parent == null)
            collision.transform.parent = transform;
    }*/

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Corpse")
            collision.transform.parent = null;
    }
}
