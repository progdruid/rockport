using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class Corpse : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    [Header("Settings")]
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float gravity = 50f;
    [SerializeField] private float deceleration = 100f;
    
    [Header("Touches")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionCheckDistance = 0.2f;
    [SerializeField] private float collisionGap = 0.01f;
    
    [Header("Cling")]
    [SerializeField] private Transform leftClingToAnchor;
    [SerializeField] private Transform rightClingToAnchor;
    
    [Header("Effects")] 
    [SerializeField] private float landingEffectHeightThreshold = 1;
    [SerializeField] private GameObject landingDustPrefab;
    [SerializeField] private Transform landingDustSpawnPoint;
    
    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PolygonCollider2D polyCollider;
    
    private bool _grounded = false;
    private float _maxYDuringFall = 0;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(rb);
        Assert.IsNotNull(polyCollider);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    
    public Vector2 LeftClingLocal => leftClingToAnchor.localPosition.To2();
    public Vector2 RightClingLocal => rightClingToAnchor.localPosition.To2();
    public Vector2 Position => rb.position;
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void FixedUpdate()
    {
        if (!_grounded && _maxYDuringFall < rb.transform.position.y)
            _maxYDuringFall = rb.transform.position.y;
        
        
        var isGroundHit = CastBody(Vector2.down, collisionCheckDistance, collisionMask, out var groundHitData);
        if (isGroundHit && !_grounded)
        {
            _grounded = true;
            
            if (_maxYDuringFall - rb.transform.position.y > landingEffectHeightThreshold) 
                Instantiate(landingDustPrefab, landingDustSpawnPoint.position, Quaternion.identity);
            _maxYDuringFall = 0;
        }
        else if (!isGroundHit && _grounded)
        {
            _grounded = false;
            _maxYDuringFall = rb.transform.position.y;
        }
        
        
        var horizontalSpeed = Mathf.MoveTowards(rb.linearVelocityX, 0, Time.fixedDeltaTime * deceleration);
        
        var verticalSpeed = _grounded 
            ? rb.linearVelocityY.ClampBottom(0)
            : Mathf.MoveTowards(rb.linearVelocityY, -maxFallSpeed, Time.fixedDeltaTime * gravity);
        
        rb.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
        
        
        //TODO: experiment with single 2D CCD instead of two for axes
        var predictedDeltaY = rb.linearVelocityY * Time.fixedDeltaTime;
        var vertDir = predictedDeltaY > 0 ? Vector2.up : Vector2.down;
        if (!predictedDeltaY.IsApproximately(0) 
            && CastBody(vertDir, predictedDeltaY.Abs(), collisionMask, out var vertCCDHit))
        {
            rb.position += vertDir * (vertCCDHit.distance - collisionGap);
            rb.linearVelocityY = 0;
        }
        var predictedDeltaX = rb.linearVelocityX * Time.fixedDeltaTime;
        var horDir = predictedDeltaX > 0 ? Vector2.right : Vector2.left;
        if (!predictedDeltaX.IsApproximately(0) 
            && CastBody(horDir, predictedDeltaX.Abs(), collisionMask, out var horCCDHit))
        {
            rb.position += horDir * (horCCDHit.distance - collisionGap);
            rb.linearVelocityX = 0;
        }
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool CastBody(Vector2 direction, float distance, LayerMask layer, out RaycastHit2D hit)
    {
        var originalLayer = polyCollider.gameObject.layer;
        //TODO: do something with layers
        polyCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        hit = Physics2D.CapsuleCast(polyCollider.bounds.center, polyCollider.bounds.size, CapsuleDirection2D.Vertical, 0, direction, distance, layer);

        polyCollider.gameObject.layer = originalLayer;
        return hit;
    }
}