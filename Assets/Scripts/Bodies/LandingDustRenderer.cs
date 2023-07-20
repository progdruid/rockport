using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingDustRenderer : MonoBehaviour
{
    [SerializeField] GameObject LandingDustPrefab;
    [SerializeField] BodySideTrigger BottomTrigger;
    [SerializeField] Transform SpawningPoint;
    [Space]
    [SerializeField] float SpawnCooldown = 0.1f;

    private float _lastEnterTime;
    private List<Animator> dustObjects = new ();

    private void Start() => BottomTrigger.EnterEvent += HandleLanding;
    private void OnDestroy() => BottomTrigger.EnterEvent -= HandleLanding;

    private void HandleLanding (Collider2D other)
    {
        if (_lastEnterTime == 0 || Time.time - _lastEnterTime >= SpawnCooldown)
        {
            GameObject go = Instantiate(LandingDustPrefab, SpawningPoint.position, Quaternion.identity);
            dustObjects.Add(go.GetComponent<Animator>());
        }

        _lastEnterTime = Time.time;
    }

    private void Update()
    {
        for (int i = 0; i < dustObjects.Count; i++)
        {
            if (dustObjects[i].GetCurrentAnimatorClipInfo(0)[0].clip.name == "None")
            {
                var obj = dustObjects[i].gameObject;
                dustObjects.RemoveAt(i);
                Destroy(obj);
                i--;
            }
        }
    }
}
