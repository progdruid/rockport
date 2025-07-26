using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject smokeEffectPrefab;
    [SerializeField] private float smokeTime = 0.3f;
    [SerializeField] private float corpseTime = 0.25f;
    [SerializeField] private float showTime = 0.5f;
    
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
        Player.Frozen = true;
        GameSystems.Ins.Controller.SetAllowMove(false);

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, pos.z - 0.1f), Quaternion.identity);
        yield return new WaitForSeconds(smokeTime);

        if (shouldSpawnCorpse)
        {
            var vel = rb.linearVelocity;
            var flipX = Player.Flip;
            
            GameSystems.Ins.FruitManager.DestroyFruit();
            DestroyPlayer();
            var corpse = GameSystems.Ins.CorpseManager.SpawnCorpse(pos, vel, flipX).transform;
            yield return new WaitForSeconds(corpseTime);
        }
        
        if (Player)
            DestroyPlayer();
        _killingPlayer = false;

        yield return new WaitForSeconds(showTime);
        
        
        
        SpawnPlayer();
        GameSystems.Ins.Controller.SetAllowMove(true);    
    }
}
