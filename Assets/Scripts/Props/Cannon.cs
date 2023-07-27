using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    public float timePeriod;
    public float offset;
    public GameObject ProjectilePrefab;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (signal != null && signal.activated)//cuz by activating signal you're disabling cannons
            return;

        if ((Time.time + timePeriod - offset)%timePeriod <= Time.deltaTime)
            Shoot();
    }

    private void Shoot ()
    {
        if (animator != null)
            animator.SetTrigger("Shot");
        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float x = transform.position.x - Mathf.Sin(angle) * 0.3f;
        float y = transform.position.y + Mathf.Cos(angle) * 0.3f;
        Vector3 spawnPoint = new Vector3(x, y, -0.5f);
        Transform projTransform = Instantiate(ProjectilePrefab, spawnPoint, Quaternion.identity).transform;
        CannonProjectile proj = projTransform.GetComponent<CannonProjectile>();
        proj.SetDirection(new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle))); 
        Registry.ins.lm.AttachToLevelAsChild(projTransform);
    }
}
