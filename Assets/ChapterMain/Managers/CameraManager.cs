using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(Rigidbody2D))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] float boxSize;

    [SerializeField] float boxTopHalfSize;
    [SerializeField] float boxBottomHalfSize;
    [SerializeField] float boxRightHalfSize;
    [SerializeField] float boxLeftHalfSize;

    private Transform target;

    private void OnEnable() => GameSystems.ins.cameraManager = this;
    private void Start()
    {
        GameSystems.ins.playerManager.PlayerSpawnEvent += HandlePlayerSpawn;
    }

    private void OnDestroy()
    {
        GameSystems.ins.playerManager.PlayerSpawnEvent -= HandlePlayerSpawn;
    }

    void Update()
    {
        if (GameSystems.ins.transitionVeil.closed || target == null)
            return;
        
        float newPosX = Mathf.Clamp(transform.position.x, target.position.x - boxLeftHalfSize, target.position.x + boxRightHalfSize);
        float newPosY = Mathf.Clamp(transform.position.y, target.position.y - boxBottomHalfSize, target.position.y + boxTopHalfSize);

        transform.position = new Vector3(newPosX, newPosY, transform.position.z);
    }

    public void SetTarget (Transform target)
    {
        this.target = target;
    }

    public void SetAndMoveToTarget (Transform target)
    {
        this.target = target;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private void HandlePlayerSpawn(GameObject player) => SetAndMoveToTarget(player.transform);
}
