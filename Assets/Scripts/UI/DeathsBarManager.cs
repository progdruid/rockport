using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsBarManager : MonoBehaviour
{
    public GameObject DeathBar;
    [Space]
    public GameObject EmptySkullPrefab;
    public GameObject FilledSkullPrefab;

    private LevelManager gm;
    private List<GameObject> skulls;

    private void Start()
    {
        skulls = new List<GameObject>();
        gm = GetComponent<LevelManager>();
        gm.CorpsesUpdateEvent += UpdateBar;
        Registry.ins.corpseManager.CorpseUpdateEvent += UpdateBar;
    }

    private void OnDestroy()
    {
        gm.CorpsesUpdateEvent -= UpdateBar;
        Registry.ins.corpseManager.CorpseUpdateEvent -= UpdateBar;
    }

    public void UpdateBar ()
    {
        int corpseCount = Registry.ins.corpseManager.GetCorpseCount();
        skulls.ForEach((GameObject skull) => Destroy(skull));
        for (int i = 0; i < gm.MaxDeaths; i++)
        {
            var skull = Instantiate(i < corpseCount ? FilledSkullPrefab : EmptySkullPrefab);
            skull.transform.SetParent(DeathBar.transform);
            skulls.Add(skull);
        }
    }

    public void SetActive (bool active) => DeathBar.SetActive (active);
}
