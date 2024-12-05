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

    public Player player { get; private set; }
    private Rigidbody2D _rb => player.GetComponent<Rigidbody2D>();
    private Vector2 _spawnPoint = Vector2.zero;
    private bool _killingPlayer;

    private void Awake()
    {
        GameSystems.ins.playerManager = this;
        //this is an exception, this event will be invoked on Start, so there is no other option than finding it manually
        GetComponent<ChapterLoader>().LevelInstantiationEvent += TryFindAndSetSpawnPoint;
    }

    private void Start() 
    {
        GameSystems.ins.inputSet.KillPlayerKeyPressEvent += KillPlayer;
    }
    private void OnDestroy()
    {
        GameSystems.ins.inputSet.KillPlayerKeyPressEvent -= KillPlayer;
        GameSystems.ins.lm.LevelInstantiationEvent -= TryFindAndSetSpawnPoint;
    }
    public void SetSpawnPoint (Vector2 pos) => _spawnPoint = pos;
    
    public void SpawnPlayer ()
    {
        if (player != null)
        {
            #if UNITY_EDITOR 
            Debug.LogError("There is already one instance of player.");
            #endif
            
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(_spawnPoint.x, _spawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        PlayerSpawnEvent(player.gameObject);
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
        Vector2 vel = _rb.linearVelocity;
        Vector3 pos = player.transform.position;
        bool flipX = player.GetComponent<SpriteRenderer>().flipX;

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, smokeEffectPrefab.transform.position.z), Quaternion.identity);

        _rb.constraints = RigidbodyConstraints2D.FreezePosition;
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
        Destroy(player.gameObject);
        player = null;
    }

    public void ResetJumpCooldown()
    {
        if (player != null)
            player.ResetJumpCooldown();
    }

    private void TryFindAndSetSpawnPoint ()
    {
        GameObject foundObject = GetComponent<ChapterLoader>().TryFindObjectWithTag("SpawnPoint");

        if (foundObject != null)
            SetSpawnPoint(foundObject.transform.position);
        else
            SetSpawnPoint(Vector2.zero);
    }
}
