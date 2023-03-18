using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(UniversalTrigger))]
public class FinishDoor : MonoBehaviour
{
    private UniversalTrigger trigger;

    #region ceremony
    private void Start()
    {
        trigger = GetComponent<UniversalTrigger>();
        trigger.EnterEvent += HandleTriggerEnter;
    }

    private void OnDestroy()
    {
        trigger.EnterEvent -= HandleTriggerEnter;
    }
    #endregion

    private void HandleTriggerEnter (Collider2D col, TriggeredType type)
    {
        if (type != TriggeredType.Player)
            return;

#if UNITY_EDITOR
        if (Registry.ins.lm.loadDirectly)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
#endif

        int currentId = Registry.ins.lm.loadLevelID;
        if (Registry.ins.lm.levelTree.GetLevelIndex(currentId + 1) == -1)
        {
            SceneManager.LoadScene("Menu");
            return;
        }

        Registry.ins.lm.loadLevelID = currentId + 1;
        Registry.ins.lm.LoadLevel();
    }/*

    private void OnTriggerEnter2D(Collider2D col)
    {
        bool signFound = col.TryGetComponent(out SignComponent sign);
        if (!signFound || !sign.HasSign("Player"))
            return;


    }*/
}
