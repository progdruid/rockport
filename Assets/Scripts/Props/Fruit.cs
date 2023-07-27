using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class Fruit : MonoBehaviour
{
    private UniversalTrigger _trigger;
    private Animator _animator;

    #region ceremony
    private void Start()
    {
        _trigger = GetComponent<UniversalTrigger>();
        _animator = GetComponent<Animator>();
        _trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        _trigger.EnterEvent -= HandleTriggerEnter;
    }
    #endregion

    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            StartCoroutine(Collect());
    }

    private IEnumerator Collect ()
    {
        _trigger.EnterEvent -= HandleTriggerEnter;
        _animator.SetTrigger("Collect");
        Registry.ins.skullManager.AddSkull();
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }
}
