using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    public float Speed;

    private void Update()
    {
        transform.localPosition += Vector3.up * Speed * Time.deltaTime;
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
