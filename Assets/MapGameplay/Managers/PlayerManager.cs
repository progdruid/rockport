using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject smokeEffectPrefab;
    [SerializeField] private float smokeTimeOffset;
    [SerializeField] private GameplayController controller;
    
    private Vector2 _spawnPoint = Vector2.zero;
    private float _spawnZ = -1;
    private bool _killingPlayer;


    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        GameSystems.Ins.PlayerManager = this;
        
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action<GameObject> PlayerSpawnEvent;

    public Player Player { get; private set; }

    public void SetSpawnPoint (Vector2 pos) => _spawnPoint = pos;
    public void SetSpawnZ(float z) => _spawnZ = z;
    
    public void SpawnPlayer ()
    {
        if (Player)
            return;
        
        Player = Instantiate(playerPrefab, new Vector3(_spawnPoint.x, _spawnPoint.y, _spawnZ), Quaternion.identity).GetComponent<Player>();
        GameSystems.Ins.GameplayCamera.SetTarget(Player.transform);
        PlayerSpawnEvent?.Invoke(Player.gameObject);
    }

    public void KillPlayer ()
    {
        if (_killingPlayer)
            return;
        _killingPlayer = true;
        Player.PrepareForDeath();
        //TODO: call player death event

        var canSpawnCorpse = GameSystems.Ins.FruitManager.GetFruitsAmount() != 0;
        StartCoroutine(KillPlayerRoutine(canSpawnCorpse));
    }

    //TODO: should be private, another function for clearing should be made
    public void DestroyPlayer ()
    {
        Destroy(Player.gameObject);
        Player = null;
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator KillPlayerRoutine(bool shouldSpawnCorpse)
    {
        var rb = Player.Body;
        var pos = Player.transform.position;

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, smokeEffectPrefab.transform.position.z),
            Quaternion.identity);

        rb.constraints |= RigidbodyConstraints2D.FreezePosition;
        controller.AllowMove = false;

        yield return new WaitForSeconds(smokeTimeOffset);

        if (shouldSpawnCorpse)
        {
            var vel = rb.linearVelocity;
            var flipX = Player.Flip;
            
            GameSystems.Ins.FruitManager.DestroyFruit();
            DestroyPlayer();
            var corpse = GameSystems.Ins.CorpseManager.SpawnCorpse(pos, vel, flipX).transform;
            GameSystems.Ins.GameplayCamera.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }
        
        yield return GameSystems.Ins.TransitionVeil.TransiteIn();
        
        if (Player)
            DestroyPlayer();
        _killingPlayer = false;
        
        SpawnPlayer();
        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        controller.AllowMove = true;    
    }
}
