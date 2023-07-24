using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class PlatformTrigger : MonoBehaviour
{
    public Collider2D platformCollider;

    private UniversalTrigger trigger;
    private Collider2D triggerCollider;

    private Collider2D[] collidersGot = new Collider2D[10];
    private Dictionary<Collider2D, bool> colliderMap = new();

    private ContactFilter2D filter;

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
        trigger.ExitEvent += HandleTriggerExit;

        filter = new();
        filter.useTriggers = false;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        trigger.ExitEvent -= HandleTriggerExit;

        Registry.ins.corpseManager.NewCorpseEvent -= HandleNewBody;
        Registry.ins.playerManager.PlayerSpawnEvent -= HandleNewBody;
    }
    #endregion
    /*
    private void FixedUpdate()
    {
        int resNumber = triggerCollider.OverlapCollider(filter, collidersGot);

        for (int i = 0; i < resNumber; i++)
        {
            bool got = colliderMap.ContainsKey(collidersGot[i]);
            if (got)
                colliderMap[collidersGot[i]] = true;
            else
            {
                colliderMap.Add(collidersGot[i], true);
                IgnoreBody(collidersGot[i], false);
            }
        }

        var arr = colliderMap.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].Key == null)
            {
                colliderMap.Remove(arr[i].Key);
                continue;
            }
            if (!arr[i].Value)
            {
                IgnoreBody(arr[i].Key, true);
                colliderMap.Remove(arr[i].Key);
            }
            colliderMap[arr[i].Key] = false;
        }

        /*for (int i = 0; i < collidersAccounted.Count; i++)
            if (!triggerCollider.IsTouching(collidersAccounted[i]))
            {
                IgnoreBody(collidersAccounted[i], true);
                collidersAccounted.RemoveAt(i);
                i--;
            }
    }*/
    
    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
        {
	        IgnoreBody(other, false);
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
    
    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
            IgnoreBody(other, true);
    }//*/

    private void HandleNewBody(GameObject body)
    {
        IgnoreBody(body.GetComponent<Collider2D>(), true);
    }
}
