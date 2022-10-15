using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsBarManager : MonoBehaviour
{
    public GameObject DeathBar;
    [Space]
    public GameObject SkullPrefab;

    private List<GameObject> skulls;

    private void Awake() => Registry.ins.deathsBar = this;

    private void Start()
    {
        skulls = new List<GameObject>();
        Registry.ins.skullManager.SkullUpdateEvent += UpdateBar;
    }

    private void OnDestroy()
    {
        Registry.ins.skullManager.SkullUpdateEvent -= UpdateBar;
    }

    public void UpdateBar ()
    {
        int corpseCount = Registry.ins.corpseManager.GetCorpseCount();
        skulls.ForEach((GameObject skull) => Destroy(skull));
        skulls.Clear();
        for (int i = 0; i < Registry.ins.skullManager.GetSkullsAmount(); i++)
        {
            var skull = Instantiate(SkullPrefab);
            skull.transform.SetParent(DeathBar.transform);
            skulls.Add(skull);
        }
    }

    public void SetActive (bool active) => DeathBar.SetActive (active);
}
