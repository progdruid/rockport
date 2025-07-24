using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameplayCamera : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float followSpeed = 5f;
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
    }

    private void OnEnable() => GameSystems.Ins.GameplayCamera = this;

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetTarget (Transform target) => this._target = target;
    public void SetAbsolutePosition (Vector2 position) => transform.SetWorldXY(position);

    public void SetModeStatic () { _isFollowing = false; _staticDirection = Vector2.zero; }
    public void SetModeMove(Vector2 direction) { _isFollowing = false; _staticDirection = direction; }
    public void SetModeFollow () { _isFollowing = true; }
    public void SetStaticSpeedMultiplier (float multiplier) => _staticSpeedMultiplier = multiplier;
    public void SetZoom (float zoom) => _nextFrameZoom = zoom;
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void LateUpdate()
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
            var pos = Vector2.Lerp(transform.position.To2(), _target.position.To2(), followSpeed * Time.deltaTime);
            transform.SetWorldXY(pos);
        }
        else
        {
            transform.position += (Vector3)_staticDirection * (staticSpeed * _staticSpeedMultiplier * Time.deltaTime);
        }
    }

}
