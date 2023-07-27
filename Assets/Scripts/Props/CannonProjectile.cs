using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class CannonProjectile : MonoBehaviour
{
    public float Speed;

    private UniversalTrigger _trigger;
    private Animator _animator;

    private bool _active = true;
    private Vector2 _direction = Vector2.zero;

    public void SetDirection (Vector2 dir) => _direction = dir;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _trigger = GetComponent<UniversalTrigger>();
        _trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        _trigger.EnterEvent -= HandleTriggerEnter;
    }

    private void FixedUpdate()
    {
        if (_active)
            transform.localPosition += (Vector3)_direction * Speed * Time.fixedDeltaTime;
    }

    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            Registry.ins.playerManager.KillPlayer();

        if (!other.isTrigger)
            StartCoroutine(CollisionRoutine(other));
    }

    private IEnumerator CollisionRoutine (Collider2D other)
    {
        _animator.SetTrigger("Collided");
        GetComponent<Collider2D>().enabled = false;
        _active = false;
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        Destroy(gameObject);
    }
}
