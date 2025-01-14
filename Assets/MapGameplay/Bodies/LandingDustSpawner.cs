using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingDustSpawner : MonoBehaviour
{
    [SerializeField] GameObject LandingDustPrefab;
    [SerializeField] BodySideTrigger BottomTrigger;
    [SerializeField] Transform SpawningPoint;
    [Space]
    [SerializeField] float SpawnCooldown = 0.1f;

    private float _lastEnterTime;

    private void Start() => BottomTrigger.EnterEvent += HandleLanding;
    private void OnDestroy() => BottomTrigger.EnterEvent -= HandleLanding;

    private void HandleLanding (Collider2D other, TriggeredType type)
    {
        if (_lastEnterTime == 0 || Time.time - _lastEnterTime >= SpawnCooldown)
            Instantiate(LandingDustPrefab, SpawningPoint.position, Quaternion.identity);

        _lastEnterTime = Time.time;
    }
}
