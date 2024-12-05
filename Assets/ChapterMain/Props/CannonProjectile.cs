using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class CannonProjectile : MonoBehaviour
{
    [SerializeField] float speed;

    private UniversalTrigger trigger;
    private Animator animator;

    private bool active = true;
    private Vector2 direction = Vector2.zero;

    public void SetDirection (Vector2 dir) => direction = dir;

    private void Start()
    {
        animator = GetComponent<Animator>();
        trigger = GetComponent<UniversalTrigger>();
        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }

    private void FixedUpdate()
    {
        if (active)
            transform.localPosition += (Vector3)direction * speed * Time.fixedDeltaTime;
    }

    private void HandleTriggerEnter (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            GameSystems.ins.playerManager.KillPlayer();

        if (!other.isTrigger)
            StartCoroutine(CollisionRoutine(other));
    }

    private IEnumerator CollisionRoutine (Collider2D other)
    {
        animator.SetTrigger("Collided");
        GetComponent<Collider2D>().enabled = false;
        active = false;
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        Destroy(gameObject);
    }
}
