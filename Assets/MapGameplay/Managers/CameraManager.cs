using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(Rigidbody2D))]
public class CameraManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private float boxSize;

    [SerializeField] private float boxTopHalfSize;
    [SerializeField] private float boxBottomHalfSize;
    [SerializeField] private float boxRightHalfSize;
    [SerializeField] private float boxLeftHalfSize;

    private Transform _target;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        ObservationHeight = transform.position.z;
    }
    private void OnEnable() => GameSystems.Ins.CameraManager = this;
    private void Start()
    {
        GameSystems.Ins.PlayerManager.PlayerSpawnEvent += HandlePlayerSpawn;
    }

    private void OnDestroy()
    {
        GameSystems.Ins.PlayerManager.PlayerSpawnEvent -= HandlePlayerSpawn;
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public float ObservationHeight { get; set; }

    public void SetTarget (Transform target)
    {
        this._target = target;
    }
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    void Update()
    {
        if (GameSystems.Ins.TransitionVeil.closed || _target == null)
            return;
        
        var newPosX = Mathf.Clamp(transform.position.x, _target.position.x - boxLeftHalfSize, _target.position.x + boxRightHalfSize);
        var newPosY = Mathf.Clamp(transform.position.y, _target.position.y - boxBottomHalfSize, _target.position.y + boxTopHalfSize);

        transform.position = new Vector3(newPosX, newPosY, ObservationHeight-2);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void SetAndMoveToTarget (Transform target)
    {
        this._target = target;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private void HandlePlayerSpawn(GameObject player) => SetAndMoveToTarget(player.transform);
}
