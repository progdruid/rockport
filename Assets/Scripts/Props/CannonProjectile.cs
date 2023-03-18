using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class CannonProjectile : MonoBehaviour
{
    public float Speed;
    private Vector3 dir;

    private UniversalTrigger trigger;

    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        trigger.EnterEvent += HandleTriggerEnter;

        float x = -Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        float y = Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        dir = new Vector3(x, y, 0f);
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }

    private void Update()
    {
        transform.localPosition += dir * Speed * Time.deltaTime;
    }

    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            Registry.ins.playerManager.KillPlayer();

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
