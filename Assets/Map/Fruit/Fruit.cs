using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Fruit : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private UnityEvent onCollect;
    
    [SerializeField] private UniversalTrigger trigger;
    [SerializeField] private Animator animator;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(trigger);
        Assert.IsNotNull(animator);
        
        trigger.EnterEvent += HandleTriggerEnter;
    }
    public override void Initialise() { }
    public override void Activate() { }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "fruit";
    public override IEnumerator<PropertyHandle> GetProperties() { yield break; }
    public override void Replicate(JSONNode data) { }
    public override JSONNode ExtractData() { return new JSONObject(); }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player)
            StartCoroutine(Collect());
    }

    private IEnumerator Collect ()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        animator.SetTrigger("Collect");
        onCollect.Invoke();
        GameSystems.Ins.FruitManager.AddFruit();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }
}
