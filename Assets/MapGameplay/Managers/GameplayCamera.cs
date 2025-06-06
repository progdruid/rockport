using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameplayCamera : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float boxSize;
    [Space]
    [SerializeField] private float boxTopHalfSize;
    [SerializeField] private float boxBottomHalfSize;
    [SerializeField] private float boxRightHalfSize;
    [SerializeField] private float boxLeftHalfSize;
    [Space]
    [SerializeField] private float staticSpeed;
    
    private Camera _camera;
    
    private Transform _target;
    private bool _isFollowing = true;
    private Vector2 _staticDirection = Vector2.zero;
    private float _staticSpeedMultiplier = 1f;
    private float _nextFrameZoom = 0f;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ObservationHeight = transform.position.z;
    }

    private void OnEnable() => GameSystems.Ins.GameplayCamera = this;

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public float ObservationHeight { get; set; }

    public void SetTarget (Transform target)
    {
        this._target = target;
        if (_isFollowing)
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
    
    public void SetModeStatic () { _isFollowing = false; _staticDirection = Vector2.zero; }
    public void SetModeMove(Vector2 direction) { _isFollowing = false; _staticDirection = direction; }
    public void SetModeFollow () { _isFollowing = true; }
    public void SetStaticSpeedMultiplier (float multiplier) => _staticSpeedMultiplier = multiplier;
    public void SetZoom (float zoom) => _nextFrameZoom = zoom;
    
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (GameSystems.Ins.TransitionVeil.closed || !_target)
            return;

        if (!_nextFrameZoom.IsApproximately(0f))
        {
            _camera.orthographicSize += _nextFrameZoom * Time.deltaTime;
            _nextFrameZoom = 0f;
        }
            
        if (_isFollowing)
        {
            var newPosX = Mathf.Clamp(transform.position.x, _target.position.x - boxLeftHalfSize, _target.position.x + boxRightHalfSize);
            var newPosY = Mathf.Clamp(transform.position.y, _target.position.y - boxBottomHalfSize, _target.position.y + boxTopHalfSize);

            transform.position = new Vector3(newPosX, newPosY, ObservationHeight-2);
        }
        else
        {
            transform.position += (Vector3)_staticDirection * (staticSpeed * _staticSpeedMultiplier * Time.deltaTime);
        }
    }

}
