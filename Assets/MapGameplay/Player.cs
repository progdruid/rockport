using System;
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
    
    [Header("Cling")]
    [SerializeField] private Transform leftClingWithAnchor;
    [SerializeField] private Transform rightClingWithAnchor;
    [SerializeField] private LayerMask clingMask;
    [SerializeField] private float clingMoveMultiplier = 0.5f;
    
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
    
    private Corpse _clungCorpse = null;
    private Vector2 _clingOffset = Vector2.zero;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(rb);
        Assert.IsNotNull(capsule);
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(animator);
        Assert.IsNotNull(soundEmitter);
        Assert.IsNotNull(soundPlayer);
        
        Assert.IsNotNull(leftClingWithAnchor);
        Assert.IsNotNull(rightClingWithAnchor);
        
        Assert.IsNotNull(landingDustPrefab);
        Assert.IsNotNull(landingDustSpawnPoint);
        
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
    public bool Flip => spriteRenderer.flipX;
    
    public float HorizontalOrderDirection { get; set; }
    public bool OrderedToCling { get; set; }
    
    public void PrepareForDeath()
    {
        gameObject.layer = 10;
        animator.SetBool(DiedAnimatorPropertyID, true);
    }
    
    public void MakeRegularJump()
    {
        _timeTriedJumping = Time.time;
        if ((_grounded || _timeUngrounded + coyoteTime > Time.time) && !_clungCorpse)
            Jump(jumpKick);
    }

    public void SuppressJump()
    {
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * suppressFactor);
    }

    private Vector2 _lastFramePos = Vector2.zero;
    private Vector2 _savedVelocity = Vector2.zero;
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void FixedUpdate()
    {
        rb.linearVelocity = _savedVelocity;
        
        if (_clungCorpse)
            Debug.Log("Debug: " + (_clungCorpse.Position.x - rb.position.x));
        _lastFramePos = rb.position;

        
        //update max y since ungrounded
        if (!_grounded && _maxYDuringFall < rb.transform.position.y)
            _maxYDuringFall = rb.transform.position.y;
        
        
        var isGroundHit = CastBodyTo(Vector2.down, collisionCheckDistance, collisionMask, out var groundHitData);
        var isGroundDirt = isGroundHit && groundHitData.collider.CompareTag("Dirt");
        
        if (isGroundHit && !_grounded)
        {
            animator.SetBool(JumpedAnimatorPropertyID, false);
            if (_timeTriedJumping + jumpBufferTime > Time.time && !_clungCorpse)
                Jump(jumpKick);
            
            soundPlayer.SelectClip(isGroundDirt ? "WalkDirt" : "WalkSolid");
            if (_maxYDuringFall - rb.transform.position.y > landingEffectHeightThreshold)
            {
                Instantiate(landingDustPrefab, landingDustSpawnPoint.position, Quaternion.identity);
                soundEmitter.EmitSound(isGroundDirt ? "LandingDirt" : "LandingSolid");
            }
            
            _maxYDuringFall = 0;
            _grounded = true;
            _timeUngrounded = float.NegativeInfinity;
        }
        else if (!isGroundHit && _grounded)
        {
            _grounded = false;
            _timeUngrounded = Time.time;
            _maxYDuringFall = rb.transform.position.y;
            animator.SetBool(JumpedAnimatorPropertyID, false);
            soundPlayer.UnselectClip();
        }

        
        
        var hor = HorizontalOrderDirection;
        var moveOrdered = !Mathf.Approximately(hor, 0);
        var facingRight = moveOrdered ? hor > 0 : spriteRenderer.flipX;

        
        if (OrderedToCling && !_clungCorpse
            && CastBodyTo(facingRight ? Vector2.right : Vector2.left, collisionCheckDistance, clingMask, out var clingHit))
        {
            var corpse = clingHit.collider.GetComponentInParent<Corpse>();
            Assert.IsNotNull(corpse);
            var offset = facingRight
                ? corpse.LeftClingLocal - rightClingWithAnchor.localPosition.To2()
                : corpse.RightClingLocal - leftClingWithAnchor.localPosition.To2();
            if (!CastBodyAt(corpse.Position + offset, collisionMask, out var _))
            {
                _clungCorpse = corpse;
                _clungCorpse.IsClung = true;
                _clungCorpse.IgnoredObject = gameObject;
                _clingOffset = offset;
            }
        }
        else if (!OrderedToCling && _clungCorpse)
        {
            _clungCorpse.IsClung = false;
            _clungCorpse.IgnoredObject = null;
            _clungCorpse = null;
        }

        
        if (moveOrdered && hor * rb.linearVelocityX < 0) //different directions -> different signs -> mult is negative
            rb.linearVelocityX = 0;

        var targetHorSpeed = hor * maxSpeed;
        if (_clungCorpse) 
            targetHorSpeed *= clingMoveMultiplier;
        rb.linearVelocityX = moveOrdered
            ? Mathf.MoveTowards(rb.linearVelocityX,  targetHorSpeed, Time.fixedDeltaTime * acceleration)
            : Mathf.MoveTowards(rb.linearVelocityX, 0, Time.fixedDeltaTime * deceleration);
        
        rb.linearVelocityY = _grounded 
            ? rb.linearVelocityY.ClampBottom(0)
            : Mathf.MoveTowards(rb.linearVelocityY, -maxFallSpeed, Time.fixedDeltaTime * gravity);
        
        animator.SetBool(RunningAnimatorPropertyID, moveOrdered);
        animator.SetBool(GroundedAnimatorPropertyID, _grounded);

        spriteRenderer.flipX = moveOrdered 
            ? hor < 0 
            : spriteRenderer.flipX;
        
        //TODO: stop calling it every frame
        if (moveOrdered)
            soundPlayer.PlayAll();
        else
            soundPlayer.Stop();
        
        
        //vertical ccd
        var predictedDeltaY = rb.linearVelocityY * Time.fixedDeltaTime;
        var vertDir = predictedDeltaY > 0 ? Vector2.up : Vector2.down;
        if (!predictedDeltaY.IsApproximately(0))
        {
            var hit = false;
            var hitDist = float.MaxValue;
            if (CastBodyTo(vertDir, predictedDeltaY.Abs(), collisionMask, out var bodyData))
            {
                hitDist = bodyData.distance;
                hit = true;
            }

            if (_clungCorpse 
                && _clungCorpse.CastBodyTo(vertDir, predictedDeltaY.Abs(), collisionMask, out var corpseData)
                && corpseData.distance < hitDist)
            {
                hitDist = corpseData.distance;
                hit = true;
            }

            if (hit)
            {
                rb.position += vertDir * (hitDist - collisionGap);
                if (_clungCorpse)
                    _clungCorpse.Position += vertDir * (hitDist - collisionGap);
                rb.linearVelocityY = 0;
            }
        }
        
        //horizontal ccd
        var predictedDeltaX = rb.linearVelocityX * Time.fixedDeltaTime;
        var horDir = predictedDeltaX > 0 ? Vector2.right : Vector2.left;
        if (!predictedDeltaX.IsApproximately(0))
        {
            var hit = false;
            var hitDist = float.MaxValue;
            if (CastBodyTo(horDir, predictedDeltaX.Abs(), collisionMask, out var bodyData))
            {
                hitDist = bodyData.distance;
                hit = true;
            }

            if (_clungCorpse 
                && _clungCorpse.CastBodyTo(horDir, predictedDeltaX.Abs(), collisionMask, out var corpseData)
                && corpseData.distance < hitDist)
            {
                hitDist = corpseData.distance;
                hit = true;
            }

            if (hit)
            {
                rb.position += horDir * (hitDist - collisionGap);
                if (_clungCorpse)
                    _clungCorpse.Position += horDir * (hitDist - collisionGap);
                rb.linearVelocityX = 0;
            }
        }
        
        // experimental directional ccd
        var predictedDelta = rb.linearVelocity * Time.fixedDeltaTime;
        var distance = predictedDelta.magnitude;
        var direction = predictedDelta.normalized;
        if (!distance.IsApproximately(0)
            && CastBodyTo(direction, distance, collisionMask, out var directionCCDHit))
        {
            var hit = false;
            var hitNormal = Vector2.zero;
            var hitDist = float.MaxValue;
            if (CastBodyTo(direction, distance, collisionMask, out var bodyData))
            {
                hitDist = bodyData.distance;
                hitNormal = bodyData.normal;
                hit = true;
            }
            if (_clungCorpse 
                && _clungCorpse.CastBodyTo(direction, distance, collisionMask, out var corpseData) 
                && corpseData.distance < hitDist)
            {
                hitDist = corpseData.distance;
                hitNormal = corpseData.normal;
                hit = true;
            }

            if (hit)
            {
                rb.position += direction * (hitDist - collisionGap);
                if (_clungCorpse)
                    _clungCorpse.Position += direction * (hitDist - collisionGap);
                rb.linearVelocity -= Vector2.Dot(rb.linearVelocity, hitNormal) * hitNormal;
                rb.linearVelocity *= 0.5f;
            }
        }
        
        rb.position += rb.linearVelocity * Time.fixedDeltaTime;
        if (_clungCorpse)
            _clungCorpse.transform.position = capsule.transform.position - _clingOffset.To3();

        _savedVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        if (_clungCorpse)
        {
            _clungCorpse.VelocityX = rb.linearVelocityX;
            _clungCorpse.VelocityY = rb.linearVelocityY;
        }
    }

    private void LateUpdate()
    {
        //if (_clungCorpse)
        //    _clungCorpse.transform.position = capsule.transform.position - _clingOffset.To3();
    }
    
    


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Jump(float kick)
    {
        rb.linearVelocityY = kick;
        _savedVelocity.y = kick;
        animator.SetBool(JumpedAnimatorPropertyID, true);
        soundEmitter.EmitSound("Jump");
    }

    private bool CastBodyTo(Vector2 direction, float distance, LayerMask layer, out RaycastHit2D hit)
    {
        //TODO: do something with layers
        var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        var originalLayer = capsule.gameObject.layer;
        var originalClungLayer = _clungCorpse?.gameObject.layer ?? ignoreLayer;
        
        capsule.gameObject.layer = ignoreLayer;
        if (_clungCorpse) _clungCorpse.gameObject.layer = ignoreLayer;

        hit = Physics2D.CapsuleCast(rb.position + capsule.offset, capsule.size, capsule.direction, 0, direction, distance, layer);

        capsule.gameObject.layer = originalLayer;
        if (_clungCorpse) _clungCorpse.gameObject.layer = originalClungLayer;
        
        return hit;
    }
    
    private bool CastBodyAt(Vector2 position, LayerMask layer, out RaycastHit2D hit)
    {
        //TODO: do something with layers
        var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        var originalLayer = capsule.gameObject.layer;
        var originalClungLayer = _clungCorpse?.gameObject.layer ?? ignoreLayer;
        
        capsule.gameObject.layer = ignoreLayer;
        if (_clungCorpse) _clungCorpse.gameObject.layer = ignoreLayer;
        
        hit = Physics2D.CapsuleCast(position, capsule.size, capsule.direction, 0, Vector2.zero, 0, layer);
        
        capsule.gameObject.layer = originalLayer;
        if (_clungCorpse) _clungCorpse.gameObject.layer = originalClungLayer;
        
        return hit;
    }
}