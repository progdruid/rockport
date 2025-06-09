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
    [SerializeField] private LayerMask platformMask;
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
    public Vector2 Position
    {
        get => rb.position;
        set => rb.position = value;
    }

    public float VelocityX
    {
        get => rb.linearVelocityX;
        set => rb.linearVelocityX = value;
    }
    public float VelocityY
    {
        get => rb.linearVelocityY;
        set => rb.linearVelocityY = value;
    }
    public bool IsClung { get; set; } = false;
    public GameObject IgnoredObject { get; set; }
    
    private Vector2 _lastFramePos = Vector2.zero;
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void FixedUpdate()
    {
        //Debug.Log("Corpse delta: " + (rb.position.x - _lastFramePos.x));
        _lastFramePos = rb.position;
        
        if (!_grounded && _maxYDuringFall < rb.transform.position.y)
            _maxYDuringFall = rb.transform.position.y;
        
        
        var isGroundHit = CastBodyTo(Vector2.down, collisionCheckDistance, collisionMask | platformMask, out var groundHitData, false);
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
        
        if(IsClung)
            return;
        
        var horizontalSpeed = Mathf.MoveTowards(rb.linearVelocityX, 0, Time.fixedDeltaTime * deceleration);
        
        var verticalSpeed = _grounded 
            ? rb.linearVelocityY.ClampBottom(0)
            : Mathf.MoveTowards(rb.linearVelocityY, -maxFallSpeed, Time.fixedDeltaTime * gravity);
        
        rb.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
        
        
        var predictedDeltaY = rb.linearVelocityY * Time.fixedDeltaTime;
        var vertDir = predictedDeltaY > 0 ? Vector2.up : Vector2.down;
        if (!predictedDeltaY.IsApproximately(0) 
            && CastBodyTo(vertDir, predictedDeltaY.Abs(), collisionMask, out var vertCCDHit))
        {
            rb.position += vertDir * (vertCCDHit.distance - collisionGap);
            rb.linearVelocityY = 0;
        }
        var predictedDeltaX = rb.linearVelocityX * Time.fixedDeltaTime;
        var horDir = predictedDeltaX > 0 ? Vector2.right : Vector2.left;
        if (!predictedDeltaX.IsApproximately(0) 
            && CastBodyTo(horDir, predictedDeltaX.Abs(), collisionMask, out var horCCDHit))
        {
            rb.position += horDir * (horCCDHit.distance - collisionGap);
            rb.linearVelocityX = 0;
        }
        //TODO: experiment with directional 2D CCD
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool CastBodyTo(Vector2 direction, float distance, LayerMask layer, out RaycastHit2D hit, bool detectInside = true)
    {
        //TODO: do something with layers
        var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        var originalLayer = polyCollider.gameObject.layer;
        var originalClungLayer = ignoreLayer;
        if (IgnoredObject) originalClungLayer = IgnoredObject.layer;
        var originalQueriesStartInColliders = Physics2D.queriesStartInColliders;
        
        polyCollider.gameObject.layer = ignoreLayer;
        if (IgnoredObject) IgnoredObject.gameObject.layer = ignoreLayer;
        Physics2D.queriesStartInColliders = detectInside;
        
        hit = Physics2D.CapsuleCast(polyCollider.bounds.center, polyCollider.bounds.size, CapsuleDirection2D.Vertical, 0, direction, distance, layer);
        
        polyCollider.gameObject.layer = originalLayer;
        if (IgnoredObject) IgnoredObject.layer = originalClungLayer;
        Physics2D.queriesStartInColliders = originalQueriesStartInColliders;
        
        return hit;
    }
}