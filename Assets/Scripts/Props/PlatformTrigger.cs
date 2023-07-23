using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class PlatformTrigger : MonoBehaviour
{
    public Collider2D platformCollider;

    private UniversalTrigger trigger;
    private Collider2D triggerCollider;

    private List<Collider2D> collidersAccounted = new ();

    #region ceremony

    private void Awake ()
    {
        Registry.ins.corpseManager.NewCorpseEvent += HandleNewBody;
        Registry.ins.playerManager.PlayerSpawnEvent += HandleNewBody;
    }

    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        triggerCollider = GetComponent<Collider2D>();
        
        trigger.EnterEvent += HandleTriggerEnter;
        //trigger.ExitEvent += HandleTriggerExit;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        //trigger.ExitEvent -= HandleTriggerExit;

        Registry.ins.corpseManager.NewCorpseEvent -= HandleNewBody;
        Registry.ins.playerManager.PlayerSpawnEvent -= HandleNewBody;
    }
    #endregion

    private void FixedUpdate()
    {
        //if (transform.parent.name == "Platform (2)")
        //    Debug.Log(Physics2D.GetIgnoreCollision(GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>(), platformCollider));
        for (int i = 0; i < collidersAccounted.Count; i++)
            if (!triggerCollider.IsTouching(collidersAccounted[i]))
            {
                IgnoreBody(collidersAccounted[i], true);
                collidersAccounted.RemoveAt(i);
                i--;
            }
    }
    
    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
        {
	        IgnoreBody(other, false);
	        collidersAccounted.Add(other);
	    }
    }

    private void IgnoreBody(Collider2D bodyCol, bool value) 
    {
        Physics2D.IgnoreCollision(bodyCol, platformCollider, value);
        for (int i = 0; i < bodyCol.transform.childCount; i++)
            if (bodyCol.transform.GetChild(i).name == "LeftSideTrigger" || bodyCol.transform.GetChild(i).name == "RightSideTrigger")
            {
                Physics2D.IgnoreCollision(bodyCol.transform.GetChild(i).GetComponent<Collider2D>(), platformCollider, value);
            }
    }
    /*
    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
            Physics2D.IgnoreCollision(other, platformCollider, true);
    }*/

    private void HandleNewBody(GameObject body)
    {
        Physics2D.IgnoreCollision(platformCollider, body.GetComponent<Collider2D>(), true);
    }
}
