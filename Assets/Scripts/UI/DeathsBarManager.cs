using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsBarManager : MonoBehaviour
{
    [SerializeField] GameObject DeathBar;
    [Space]
    [SerializeField] GameObject SkullIconPrefab;
    [SerializeField] int MaxSkullIcons;

    private List<GameObject> skullIcons;

    private void Awake() => Registry.ins.deathsBar = this;

    private void Start()
    {
        skullIcons = new List<GameObject>();
        Registry.ins.skullManager.SkullUpdateEvent += UpdateBar;

        for (int i = 0; i < MaxSkullIcons; i++)
            CreateSkullIcon(false);
    }

    private GameObject CreateSkullIcon (bool startState)
    {
        var skull = Instantiate(SkullIconPrefab);
        skull.transform.SetParent(DeathBar.transform);
        skullIcons.Add(skull);
        skull.SetActive(startState);
        return skull;
    }

    private void OnDestroy()
    {
        Registry.ins.skullManager.SkullUpdateEvent -= UpdateBar;
    }

    public void UpdateBar ()
    {
        int skullCount = Registry.ins.skullManager.GetSkullsAmount();
        for (int i = 0; i < skullIcons.Count; i++)
        {
            if (i < skullCount)
                skullIcons[i].SetActive(true);
            else
                skullIcons[i].SetActive(false);
        }
        for (int i = 0; i < skullCount - skullIcons.Count; i++)
            CreateSkullIcon(true);
    }

    public void SetActive (bool active) => DeathBar.SetActive (active);
}
