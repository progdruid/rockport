using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    public float Speed;
    private Vector3 dir;

    private void Start()
    {
        float x = -Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        float y = Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        dir = new Vector3(x, y, 0f);
    }

    private void Update()
    {
        transform.localPosition += dir * Speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isSigned = other.gameObject.TryGetComponent(out SignComponent sign);
        if (isSigned && sign.GetSigns().Contains("Player"))
            Registry.ins.playerManager.KillPlayer();

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
