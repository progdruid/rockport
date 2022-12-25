using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    public float timePeriod;
    public float offset;
    public GameObject ProjectilePrefab;

    private Vector3 relativeProjSpawnPoint = new Vector3(0f, 0.25f, -1f);
    private Animator animator;
    private float timePassed;

    void Start()
    {
        animator = GetComponent<Animator>();
        timePassed = -offset;
    }

    void Update()
    {
        if (signal != null && signal.activated)//cuz by activating signal you're disabling cannons
            return;

        if (timePassed < timePeriod)
            timePassed += Time.deltaTime;
        else
        {
            timePassed = 0;
            Shoot();
        }
    }

    private void Shoot ()
    {
        if (animator != null)
            animator.SetTrigger("Shot");
        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float x = transform.position.x - Mathf.Sin(angle) * 0.25f;
        float y = transform.position.y + Mathf.Cos(angle) * 0.25f;
        Vector3 spawnPoint = new Vector3(x, y, 0f);
        Quaternion spawnRot = transform.rotation;
        Transform proj = Instantiate(ProjectilePrefab, spawnPoint, spawnRot).transform;
        
    }
}
