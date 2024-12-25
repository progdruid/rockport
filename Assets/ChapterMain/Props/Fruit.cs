using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniversalTrigger), typeof(Animator))]
public class Fruit : MonoBehaviour
{
    [SerializeField] UnityEvent OnCollect;

    private UniversalTrigger trigger;
    private Animator animator;

    #region ceremony
    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        animator = GetComponent<Animator>();
        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }
    #endregion

    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            StartCoroutine(Collect());
    }

    private IEnumerator Collect ()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        animator.SetTrigger("Collect");
        OnCollect.Invoke();
        GameSystems.Ins.FruitManager.AddFruit();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }
}
