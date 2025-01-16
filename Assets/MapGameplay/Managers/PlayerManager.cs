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

    private Player _player;
    
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
    public event System.Action PlayerDeathEvent;
    
    public void SetSpawnPoint (Vector2 pos) => _spawnPoint = pos;
    public void SetSpawnZ(float z) => _spawnZ = z;
    
    public void SpawnPlayer ()
    {
        if (_player)
            return;
        
        _player = Instantiate(playerPrefab, new Vector3(_spawnPoint.x, _spawnPoint.y, _spawnZ), Quaternion.identity).GetComponent<Player>();
        GameSystems.Ins.GameplayCamera.SetTarget(_player.transform);
        PlayerSpawnEvent?.Invoke(_player.gameObject);
    }

    public void KillPlayer ()
    {
        if (_killingPlayer)
            return;
        _killingPlayer = true;
        PlayerDeathEvent?.Invoke();

        var canSpawnCorpse = GameSystems.Ins.FruitManager.GetFruitsAmount() != 0;
        StartCoroutine(KillPlayerRoutine(canSpawnCorpse));
    }

    //TODO: should be private, another function for clearing should be made
    public void DestroyPlayer ()
    {
        Destroy(_player.gameObject);
        _player = null;
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator KillPlayerRoutine(bool shouldSpawnCorpse)
    {
        var rb = _player.GetComponent<Rigidbody2D>();

        var vel = rb.linearVelocity;
        var pos = _player.transform.position;
        var flipX = _player.GetComponent<SpriteRenderer>().flipX;

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, smokeEffectPrefab.transform.position.z),
            Quaternion.identity);

        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GameSystems.Ins.InputSet.Active = false;

        yield return new WaitForSeconds(smokeTimeOffset);

        if (shouldSpawnCorpse)
        {
            GameSystems.Ins.FruitManager.DestroyFruit();
            DestroyPlayer();
            var corpse = GameSystems.Ins.CorpseManager.SpawnCorpse(pos, vel, flipX).transform;
            GameSystems.Ins.GameplayCamera.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }

        yield return GameSystems.Ins.TransitionVeil.TransiteIn();

        if (!shouldSpawnCorpse)
            DestroyPlayer();
        _killingPlayer = false;

        Assert.IsNull(_player);
        SpawnPlayer();

        yield return GameSystems.Ins.TransitionVeil.TransiteOut();

        GameSystems.Ins.InputSet.Active = true;
    }
}
