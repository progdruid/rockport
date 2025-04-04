using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] UnityEvent OnIgnition;

    private Animator animator;
    private UniversalTrigger trigger;

    private bool activated;

    #region ceremony
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
    #endregion

    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if ((type != TriggeredType.Player && type != TriggeredType.Corpse) || activated)
            return;

        activated = true;
        GameSystems.Ins.PlayerManager.SetSpawnPoint(transform.position);
        OnIgnition.Invoke();
        animator.SetTrigger("Burned");
    }
}
