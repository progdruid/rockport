using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject corpsePrefab;

    private readonly List<GameObject> _corpses = new List<GameObject>();
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake() => GameSystems.Ins.CorpseManager = this;
    private void Start() => CorpseUpdateEvent?.Invoke();

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action CorpseUpdateEvent;
    public event System.Action<GameObject> NewCorpseEvent;
    public int GetCorpseCount () => _corpses.Count;
    
    public GameObject SpawnCorpse (Vector3 pos, Vector2 startVel, bool flipX)
    {
        var corpse = Instantiate(corpsePrefab, pos, Quaternion.identity);
        corpse.GetComponent<Rigidbody2D>().linearVelocity += startVel;
        corpse.GetComponent<SpriteRenderer>().flipX = flipX;
        _corpses.Add(corpse);

        CorpseUpdateEvent?.Invoke();
        NewCorpseEvent?.Invoke(corpse);

        return corpse;
    }

    public void ClearCorpses ()
    {
        for (var i = _corpses.Count - 1; i >= 0; i--)
        {
            Destroy(_corpses[i]);
            _corpses.RemoveAt(i);
        }

        CorpseUpdateEvent?.Invoke();
    }

}
