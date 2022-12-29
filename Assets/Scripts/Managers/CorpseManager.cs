using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseManager : MonoBehaviour
{
    public event System.Action CorpseUpdateEvent = delegate { };
    public event System.Action<GameObject> NewCorpseEvent = delegate { };
    public GameObject corpsePrefab;
    private List<GameObject> corpses = new List<GameObject>();

    private void Awake() => Registry.ins.corpseManager = this;
    private void Start() => CorpseUpdateEvent();

    public GameObject SpawnCorpse (Vector2 pos, Vector2 startVel)
    {
        GameObject corpse = Instantiate(corpsePrefab, pos, Quaternion.identity);
        corpse.GetComponent<Rigidbody2D>().velocity += startVel;
        corpses.Add(corpse);

        CorpseUpdateEvent();
        NewCorpseEvent(corpse);

        return corpse;
    }

    public void ClearCorpses ()
    {
        for (int i = corpses.Count - 1; i >= 0; i--)
        {
            Destroy(corpses[i]);
            corpses.RemoveAt(i);
        }

        CorpseUpdateEvent();
    }

    public int GetCorpseCount ()
    {
        return corpses.Count;
    }
}
