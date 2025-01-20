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
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask corpseLayer;

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
    
    private CorpsePhysics _hitchedCorpse;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(rb);
        Assert.IsNotNull(capsule);
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(animator);
        Assert.IsNotNull(soundEmitter);
        Assert.IsNotNull(soundPlayer);
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
        if (_hitchedCorpse) return;
        
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
        if (!_grounded && _maxYDuringFall < rb.transform.position.y)
            _maxYDuringFall = rb.transform.position.y;
        
        var isGroundHit = CheckCollision(Vector2.down, groundLayer, out var groundHitData)
            || CheckCollision(Vector2.down, corpseLayer, out groundHitData);
        var isDirt = isGroundHit && groundHitData.collider.CompareTag("Dirt");
        
        
        if (groundHitData && !_grounded)
        {
            _grounded = true;
            _timeUngrounded = float.NegativeInfinity;
            if (_timeTriedJumping + jumpBufferTime > Time.time)
                Jump(jumpKick);
            
            soundPlayer.SelectClip(isDirt ? "WalkDirt" : "WalkSolid");
            if (_maxYDuringFall - rb.transform.position.y > landingEffectHeightThreshold)
            {
                Instantiate(landingDustPrefab, landingDustSpawnPoint.position, Quaternion.identity);
                soundEmitter.EmitSound(isDirt ? "LandingDirt" : "LandingSolid");
            }
            _maxYDuringFall = 0;
        }
        else if (!groundHitData && _grounded)
        {
            _grounded = false;
            _timeUngrounded = Time.time;
            _maxYDuringFall = rb.transform.position.y;
            animator.SetBool(JumpedAnimatorPropertyID, false);
            soundPlayer.UnselectClip();
        }


        var hor = HorizontalDirection;
        var staying = Mathf.Approximately(hor, 0);

        if ((!Hitch || !_grounded) && _hitchedCorpse)
        {
            _hitchedCorpse.Unhitch();
            _hitchedCorpse = null;
        }
        else if (Hitch && !_hitchedCorpse &&
                 CheckCollision(new Vector2(spriteRenderer.flipX ? -1 : 1, 0), corpseLayer, out var hitData) &&
                 hitData.collider.TryGetComponent(out CorpsePhysics corpse))
        {
            _hitchedCorpse = corpse;
            _hitchedCorpse.Hitch(rb);
        }
        
        var horizontalSpeed = staying
            ? Mathf.MoveTowards(rb.linearVelocity.x, 0, Time.fixedDeltaTime * deceleration)
            : Mathf.MoveTowards(rb.linearVelocity.x, hor * maxSpeed, Time.fixedDeltaTime * acceleration);

        var verticalSpeed = Mathf.MoveTowards(rb.linearVelocity.y, -maxFallSpeed, Time.fixedDeltaTime * gravity);
        
        rb.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
        _hitchedCorpse?.Push(hor, Mathf.Abs(horizontalSpeed));
        
        
        
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
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Jump(float kick)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, kick);
        animator.SetBool(JumpedAnimatorPropertyID, true);
        soundEmitter.EmitSound("Jump");
    }

    private bool CheckCollision(Vector2 direction, LayerMask layer, out RaycastHit2D hit)
    {
        var cachedQueriesStartInColliders = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;
        hit = Physics2D.CapsuleCast(capsule.bounds.center, capsule.size, capsule.direction, 0, 
            direction, groundCheckDistance, layer);
        Physics2D.queriesStartInColliders = cachedQueriesStartInColliders;
        return hit;
    }
}