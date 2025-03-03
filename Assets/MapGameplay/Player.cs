using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    //static part///////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private static readonly int JumpedAnimatorPropertyID = Animator.StringToHash("Jumped");
    private static readonly int GroundedAnimatorPropertyID = Animator.StringToHash("Grounded");
    private static readonly int RunningAnimatorPropertyID = Animator.StringToHash("Running");
    private static readonly int DiedAnimatorPropertyID = Animator.StringToHash("Died");
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Jump")]
    [SerializeField] private float jumpKick = 15f;
    [SerializeField] private float suppressFactor = 0.5f;
    [SerializeField] private float coyoteTime = 0.05f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    [Header("Horizontal Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 100f;
    
    [Header("Gravity")]
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float gravity = 50f;
    
    [Header("Touches")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionCheckDistance = 0.2f;
    [SerializeField] private float collisionGap = 0.01f;

    [Header("Effects")] 
    [SerializeField] private float landingEffectHeightThreshold;
    [SerializeField] private GameObject landingDustPrefab;
    [SerializeField] private Transform landingDustSpawnPoint;
    
    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D capsule;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private CustomSoundEmitter soundEmitter;
    [SerializeField] private PermutableSoundPlayer soundPlayer;
    
    private bool _grounded;
    private float _timeUngrounded = float.NegativeInfinity;
    private float _timeTriedJumping = float.NegativeInfinity;
    private float _maxYDuringFall = 0;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(rb);
        Assert.IsNotNull(capsule);
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(animator);
        Assert.IsNotNull(soundEmitter);
        Assert.IsNotNull(soundPlayer);
        
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.freezeRotation = true;
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.angularDamping = 0;
        rb.linearVelocity = Vector2.zero;
        rb.inertia = 0;
        rb.useAutoMass = false;
        rb.mass = 0;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////

    public Rigidbody2D Body => rb;
    
    public bool Flip { get => spriteRenderer.flipX; set => spriteRenderer.flipX = value; }
    
    public float HorizontalDirection { get; set; }
    public bool Hitch { get; set; }
    
    public void PrepareForDeath()
    {
        gameObject.layer = 10;
        animator.SetBool(DiedAnimatorPropertyID, true);
    }
    
    public void MakeRegularJump()
    {
        _timeTriedJumping = Time.time;
        if (_grounded || _timeUngrounded + coyoteTime > Time.time)
            Jump(jumpKick);
    }

    public void SuppressJump()
    {
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * suppressFactor);
    }

    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void FixedUpdate()
    {
        //update max y since ungrounded
        if (!_grounded && _maxYDuringFall < rb.transform.position.y)
            _maxYDuringFall = rb.transform.position.y;
        
        
        var isGroundHit = CastBody(Vector2.down, collisionCheckDistance, collisionMask, out var groundHitData);
        var isGroundDirt = isGroundHit && groundHitData.collider.CompareTag("Dirt");
        
        if (isGroundHit && !_grounded)
        {
            _grounded = true;
            _timeUngrounded = float.NegativeInfinity;
            if (_timeTriedJumping + jumpBufferTime > Time.time)
                Jump(jumpKick);
            
            soundPlayer.SelectClip(isGroundDirt ? "WalkDirt" : "WalkSolid");
            if (_maxYDuringFall - rb.transform.position.y > landingEffectHeightThreshold)
            {
                Instantiate(landingDustPrefab, landingDustSpawnPoint.position, Quaternion.identity);
                soundEmitter.EmitSound(isGroundDirt ? "LandingDirt" : "LandingSolid");
            }
            _maxYDuringFall = 0;
        }
        else if (!isGroundHit && _grounded)
        {
            _grounded = false;
            _timeUngrounded = Time.time;
            _maxYDuringFall = rb.transform.position.y;
            animator.SetBool(JumpedAnimatorPropertyID, false);
            soundPlayer.UnselectClip();
        }

        var hor = HorizontalDirection;
        var staying = Mathf.Approximately(hor, 0);
        
        var horizontalSpeed = staying
            ? Mathf.MoveTowards(rb.linearVelocityX, 0, Time.fixedDeltaTime * deceleration)
            : Mathf.MoveTowards(rb.linearVelocityX, hor * maxSpeed, Time.fixedDeltaTime * acceleration);
        
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
        
        
        animator.SetBool(RunningAnimatorPropertyID, !staying);
        animator.SetBool(GroundedAnimatorPropertyID, _grounded);
        
        spriteRenderer.flipX = hor switch
        {
            < 0 => true,
            > 0 => false, 
            _ => spriteRenderer.flipX
        };
        
        //TODO: stop calling it every frame
        if (!staying)
            soundPlayer.PlayAll();
        else
            soundPlayer.Stop();
    }
    
    
    
    // private void OnCollisionStay2D(Collision2D collision)
    // {
    //     var totalPenetrationVector = Vector2.zero;
    //     foreach (var contact in collision.contacts) 
    //         totalPenetrationVector += contact.normal * Mathf.Abs(contact.separation);
    //     totalPenetrationVector /= collision.contactCount;
    //
    //     Debug.Log(totalPenetrationVector);
    //     rb.linearVelocity += totalPenetrationVector * penetrationResolutionSpeed;
    //     
    //     // old
    //     // var resolutionMovement = totalPenetrationVector * penetrationResolutionSpeed * Time.fixedDeltaTime;
    //     // rb.position += resolutionMovement;
    //
    //     // Optional: Implement a maximum resolution distance per frame
    //     // resolutionMovement = Vector2.ClampMagnitude(resolutionMovement, maxResolutionDistancePerFrame);
    // }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Jump(float kick)
    {
        rb.linearVelocityY = kick;
        animator.SetBool(JumpedAnimatorPropertyID, true);
        soundEmitter.EmitSound("Jump");
    }

    private bool CastBody(Vector2 direction, float distance, LayerMask layer, out RaycastHit2D hit)
    {
        var originalLayer = capsule.gameObject.layer;
        //TODO: do something with layers
        capsule.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        hit = Physics2D.CapsuleCast(capsule.bounds.center, capsule.size, capsule.direction, 0, direction, distance, layer);

        capsule.gameObject.layer = originalLayer;
        return hit;
    }
}