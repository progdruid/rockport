using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CorpsePhysics : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Settings")] 
    [SerializeField] private float pushingFactor = 0.5f;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float groundFriction = 10f;
    [SerializeField] private float correctionSpeed = 20f;
    [SerializeField] private float maxVelocityDelta = 10000f;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float gravity = 50f;
    
    [Header("Touches")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask bodyLayer;
    
    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PolygonCollider2D polyCollider;
    
    private Rigidbody2D _hitchedBody;
    private float _relativeDirection;
    
    private LayerMask _combinedLayer;
    
    private readonly HashSet<Collider2D> _topColliders = new();
    private readonly HashSet<Collider2D> _botColliders = new();
    private readonly HashSet<Collider2D> _rightColliders = new();
    private readonly HashSet<Collider2D> _leftColliders = new();

    private Vector2 _min;
    private Vector2 _max;
    private Vector2 _centre;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(rb);
        Assert.IsNotNull(polyCollider);
        
        _combinedLayer = groundLayer | bodyLayer;
        
        _min = polyCollider.bounds.center - polyCollider.bounds.size / 2;
        _max = polyCollider.bounds.center + polyCollider.bounds.size / 2;
        _centre = polyCollider.bounds.center;
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
        
        Unhitch();
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void Hitch(Rigidbody2D newHitchedBody)
    {
        if (!newHitchedBody) return;
        
        _hitchedBody = newHitchedBody;
        _relativeDirection = Mathf.Sign(transform.position.x - _hitchedBody.transform.position.x);
    }

    public void Unhitch()
    {
        _hitchedBody = null;
        rb.linearVelocityX = 0;
    }
    
    public void Push(float direction, float absoluteSpeed)
    {
        if (direction == 0 || (direction > 0 ? _rightColliders : _leftColliders).Count != 0)
            return;
        rb.linearVelocityX = Mathf.Max(absoluteSpeed * pushingFactor, baseSpeed) * direction;
    }
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void FixedUpdate()
    {
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0, groundFriction * Time.fixedDeltaTime);
        rb.linearVelocityY = _botColliders.Count > 0 ? 0f : rb.linearVelocityY - gravity * Time.fixedDeltaTime;

        if (_hitchedBody)
        {
            var target = _hitchedBody.transform.position.x + distance * _relativeDirection;
            rb.MovePosition(new Vector2(target, rb.position.y));
        }
        else
            rb.MovePosition(rb.position + rb.linearVelocity * Time.fixedDeltaTime);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts) 
        {
            var mainDiagonal = (_max.y - _min.y) / (_max.x - _min.x);
            var antiDiagonal = -mainDiagonal;

            var aboveMainDiagonal = contact.point.y - _centre.y > mainDiagonal * (contact.point.x - _centre.x);
            var aboveAntiDiagonal = contact.point.y - _centre.y > antiDiagonal * (contact.point.x - _centre.x);

            ((aboveMainDiagonal, aboveAntiDiagonal) switch
            {
                (true, true) => _topColliders,
                (false, false) => _botColliders,
                (false, true) => _leftColliders,
                (true, false) => _rightColliders
            }).Add(collision.collider);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            var pushVector = contact.normal * (Mathf.Abs(contact.separation) + 0.01f);
            rb.MovePosition(rb.position + pushVector);
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        _topColliders.Remove(collision.collider);
        _botColliders.Remove(collision.collider);
        _leftColliders.Remove(collision.collider);
        _rightColliders.Remove(collision.collider);
    }
}