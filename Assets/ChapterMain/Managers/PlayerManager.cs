using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject smokeEffectPrefab;
    [SerializeField] float smokeTimeOffset;

    public event System.Action<GameObject> PlayerSpawnEvent = delegate { };
    public event System.Action PlayerDeathEvent = delegate { };

    private Player _player;
    
    private Vector2 _spawnPoint = Vector2.zero;
    private float _spawnZ = -1;
    private bool _killingPlayer;

    private void Awake()
    {
        GameSystems.Ins.PlayerManager = this;
    }

    private void Start() 
    {
        GameSystems.Ins.InputSet.KillPlayerKeyPressEvent += KillPlayer;
    }
    private void OnDestroy()
    {
        GameSystems.Ins.InputSet.KillPlayerKeyPressEvent -= KillPlayer;
    }
    public void SetSpawnPoint (Vector2 pos) => _spawnPoint = pos;
    public void SetSpawnZ(float z) => _spawnZ = z;
    
    public void SpawnPlayer ()
    {
        Assert.IsNull(_player);

        _player = Instantiate(playerPrefab, new Vector3(_spawnPoint.x, _spawnPoint.y, _spawnZ), Quaternion.identity).GetComponent<Player>();
        PlayerSpawnEvent(_player.gameObject);
    }

    public void KillPlayer ()
    {
        if (_killingPlayer)
            return;
        _killingPlayer = true;
        PlayerDeathEvent();

        bool canSpawnCorpse = GameSystems.Ins.FruitManager.GetFruitsAmount() != 0;
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
        GameSystems.Ins.InputSet.Active = false;

        yield return new WaitForSeconds(smokeTimeOffset);

        if (shouldSpawnCorpse) 
        {
            GameSystems.Ins.FruitManager.DestroyFruit();
            DestroyPlayer();
            Transform corpse = GameSystems.Ins.CorpseManager.SpawnCorpse(pos, vel, flipX).transform;
            GameSystems.Ins.CameraManager.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }

        yield return GameSystems.Ins.TransitionVeil.TransiteIn();

        if (!shouldSpawnCorpse)
            DestroyPlayer();
        _killingPlayer = false;

        SpawnPlayer();

        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        
        //
        GameSystems.Ins.InputSet.Active = true;
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
