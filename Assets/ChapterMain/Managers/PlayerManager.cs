using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject smokeEffectPrefab;
    [SerializeField] float smokeTimeOffset;

    public event System.Action<GameObject> PlayerSpawnEvent = delegate { };
    public event System.Action PlayerDeathEvent = delegate { };

    private Player _player;
    
    private Vector2 _spawnPoint = Vector2.zero;
    private bool _killingPlayer;

    private void Awake()
    {
        GameSystems.ins.playerManager = this;
    }

    private void Start() 
    {
        GameSystems.ins.inputSet.KillPlayerKeyPressEvent += KillPlayer;
    }
    private void OnDestroy()
    {
        GameSystems.ins.inputSet.KillPlayerKeyPressEvent -= KillPlayer;
    }
    public void SetSpawnPoint (Vector2 pos) => _spawnPoint = pos;
    
    public void SpawnPlayer ()
    {
        if (_player != null)
        {
            #if UNITY_EDITOR 
            Debug.LogError("There is already one instance of player.");
            #endif
            
            return;
        }

        _player = Instantiate(playerPrefab, new Vector3(_spawnPoint.x, _spawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        PlayerSpawnEvent(_player.gameObject);
    }

    public void KillPlayer ()
    {
        if (_killingPlayer)
            return;
        _killingPlayer = true;
        PlayerDeathEvent();

        bool canSpawnCorpse = GameSystems.ins.fruitManager.GetFruitsAmount() != 0;
        StartCoroutine(KillPlayerRoutine(canSpawnCorpse));
    }

    private IEnumerator KillPlayerRoutine(bool shouldSpawnCorpse)
    {
        var rb = _player.GetComponent<Rigidbody2D>();
        
        Vector2 vel = rb.linearVelocity;
        Vector3 pos = _player.transform.position;
        bool flipX = _player.GetComponent<SpriteRenderer>().flipX;

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, smokeEffectPrefab.transform.position.z), Quaternion.identity);

        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GameSystems.ins.inputSet.Active = false;

        yield return new WaitForSeconds(smokeTimeOffset);

        if (shouldSpawnCorpse) 
        {
            GameSystems.ins.fruitManager.DestroyFruit();
            DestroyPlayer();
            Transform corpse = GameSystems.ins.corpseManager.SpawnCorpse(pos, vel, flipX).transform;
            GameSystems.ins.cameraManager.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }

        yield return GameSystems.ins.transitionVeil.TransiteIn();

        if (!shouldSpawnCorpse)
            DestroyPlayer();
        _killingPlayer = false;

        SpawnPlayer();

        yield return GameSystems.ins.transitionVeil.TransiteOut();
        
        //
        GameSystems.ins.inputSet.Active = true;
    }

    public void DestroyPlayer ()
    {
        Destroy(_player.gameObject);
        _player = null;
    }

    public void ResetJumpCooldown()
    {
        if (_player != null)
            _player.ResetJumpCooldown();
    }
}
