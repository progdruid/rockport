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
    }

    private void OnDestroy()
    {
        gm.CorpsesUpdateEvent -= UpdateBar;
    }

    public void UpdateBar (List<GameObject> corpses)
    {
        skulls.ForEach((GameObject skull) => Destroy(skull));
        for (int i = 0; i < gm.MaxDeaths; i++)
        {
            var skull = Instantiate(i < corpses.Count ? FilledSkullPrefab : EmptySkullPrefab);
            skull.transform.SetParent(DeathBar.transform);
            skulls.Add(skull);
        }
    }

    public void SetActive (bool active) => DeathBar.SetActive (active);
}
