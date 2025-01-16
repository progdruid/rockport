using UnityEngine;

public class GameplayCamera : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float boxSize;
    [Space]
    [SerializeField] private float boxTopHalfSize;
    [SerializeField] private float boxBottomHalfSize;
    [SerializeField] private float boxRightHalfSize;
    [SerializeField] private float boxLeftHalfSize;
    
    private Transform _target;
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake() => ObservationHeight = transform.position.z;
    private void OnEnable() => GameSystems.Ins.GameplayCamera = this;

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public float ObservationHeight { get; set; }

    public void SetTarget (Transform target)
    {
        this._target = target;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (GameSystems.Ins.TransitionVeil.closed || !_target)
            return;
        
        var newPosX = Mathf.Clamp(transform.position.x, _target.position.x - boxLeftHalfSize, _target.position.x + boxRightHalfSize);
        var newPosY = Mathf.Clamp(transform.position.y, _target.position.y - boxBottomHalfSize, _target.position.y + boxTopHalfSize);

        transform.position = new Vector3(newPosX, newPosY, ObservationHeight-2);
    }

}
