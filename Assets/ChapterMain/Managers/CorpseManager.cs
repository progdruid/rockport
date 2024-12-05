using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseManager : MonoBehaviour
{
    public event System.Action CorpseUpdateEvent = delegate { };
    public event System.Action<GameObject> NewCorpseEvent = delegate { };
    public GameObject corpsePrefab;
    private List<GameObject> corpses = new List<GameObject>();

    private void Awake() => GameSystems.ins.corpseManager = this;
    private void Start() => CorpseUpdateEvent();

    public GameObject SpawnCorpse (Vector2 pos, Vector2 startVel, bool flipX)
    {
        GameObject corpse = Instantiate(corpsePrefab, new Vector3(pos.x, pos.y, -1), Quaternion.identity);
        corpse.GetComponent<Rigidbody2D>().linearVelocity += startVel;
        corpse.GetComponent<CorpsePhysics>().kickedMode = true;
        corpse.GetComponent<SpriteRenderer>().flipX = flipX;
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
