using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class PlatformTrigger : MonoBehaviour
{
    public Collider2D platformCollider;

    private UniversalTrigger trigger;

    #region ceremony

    private void Awake ()
    {
        GameSystems.ins.corpseManager.NewCorpseEvent += HandleNewBody;
        GameSystems.ins.playerManager.PlayerSpawnEvent += HandleNewBody;
    }

    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();

        trigger.EnterEvent += HandleTriggerEnter;
        trigger.ExitEvent += HandleTriggerExit;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
        trigger.ExitEvent -= HandleTriggerExit;

        GameSystems.ins.corpseManager.NewCorpseEvent -= HandleNewBody;
        GameSystems.ins.playerManager.PlayerSpawnEvent -= HandleNewBody;
    }
    #endregion
    
    private void IgnoreBody(Collider2D bodyCol, bool value) 
    {
        Physics2D.IgnoreCollision(bodyCol, platformCollider, value);
        for (int i = 0; i < bodyCol.transform.childCount; i++)
            if (bodyCol.transform.GetChild(i).name == "LeftSideTrigger" || bodyCol.transform.GetChild(i).name == "RightSideTrigger")
            {
                Physics2D.IgnoreCollision(bodyCol.transform.GetChild(i).GetComponent<Collider2D>(), platformCollider, value);
            }
    }
    
    private void HandleTriggerEnter(Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
        {
	        IgnoreBody(other, false);
	    }
    }
    
    private void HandleTriggerExit (Collider2D other, TriggeredType type)
    {
        if (type == TriggeredType.Player || type == TriggeredType.Corpse)
            IgnoreBody(other, true);
    }

    private void HandleNewBody(GameObject body)
    {
        IgnoreBody(body.GetComponent<Collider2D>(), true);
    }

}
