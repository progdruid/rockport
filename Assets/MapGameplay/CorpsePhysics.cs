using UnityEngine;
using UnityEngine.Assertions;

public class CorpsePhysics : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float pushingFactor = 0.5f;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float groundFriction = 10f;
    [SerializeField] private float correctionSpeed = 20f;
    [SerializeField] private float maxVelocityDelta = 10000f;
    [SerializeField] private float distance = 1f;
    
    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D rb;
    
    private Rigidbody2D _hitchedBody;
    private float _relativeDirection;

    private void Awake()
    {
        Assert.IsNotNull(rb);
        rb.mass = float.MaxValue;
        Unhitch();
    }

    public void Hitch(Rigidbody2D hitchedBody)
    {
        if (!hitchedBody) return;
        
        _hitchedBody = hitchedBody;
        _relativeDirection = Mathf.Sign(transform.position.x - _hitchedBody.transform.position.x);
    }

    public void Unhitch()
    {
        _hitchedBody = null;
        rb.linearVelocityX = 0;
    }
    
    public void Push(float direction, float absoluteSpeed)
    {
        rb.linearVelocityX = Mathf.Max(absoluteSpeed * pushingFactor, baseSpeed) * direction;
    }
    
    private void FixedUpdate()
    {
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0, groundFriction * Time.fixedDeltaTime);
        
        if (!_hitchedBody) 
            return;

        var target = _hitchedBody.transform.position.x + distance * _relativeDirection;
        var current = transform.position.x;
        var error = target - current;
            
        var correctionVelocity = error * correctionSpeed;
        var averageVelocity = (rb.linearVelocityX + _hitchedBody.linearVelocityX) / 2f;
            
        var targetVelocity1 = averageVelocity + correctionVelocity * 0.5f;
        var targetVelocity2 = averageVelocity - correctionVelocity * 0.5f;
            
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocity1, maxVelocityDelta);
        _hitchedBody.linearVelocityX = Mathf.MoveTowards(_hitchedBody.linearVelocityX, targetVelocity2, maxVelocityDelta);
    }
}