using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Cannon : MonoBehaviour
{
    [SerializeField] SignalSource signal;
    [SerializeField] float timePeriod;
    [SerializeField] float offset;
    [SerializeField] GameObject ProjectilePrefab;
    [SerializeField] UnityEvent OnShoot;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (signal != null && signal.Activated)//because by activating signal you're disabling cannons
            return;

        if ((Time.time + timePeriod - offset)%timePeriod <= Time.deltaTime)
            Shoot();
    }

    private void Shoot ()
    {
        animator.SetTrigger("Shot");
        OnShoot.Invoke();

        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float x = transform.position.x - Mathf.Sin(angle) * 0.3f;
        float y = transform.position.y + Mathf.Cos(angle) * 0.3f;
        Vector3 spawnPoint = new Vector3(x, y, 0.01f);

        Transform projTransform = Instantiate(ProjectilePrefab, spawnPoint, Quaternion.identity).transform;

        CannonProjectile proj = projTransform.GetComponent<CannonProjectile>();
        proj.SetDirection(new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle))); 

        GameSystems.Ins.Loader.AttachToLevelAsChild(projTransform);
    }
}
