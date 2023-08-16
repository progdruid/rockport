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
        Registry.ins.playerManager = this;
        //this is an exception, this event will be invoked on Start, so there is no other option that finding it manually
        GetComponent<LevelLoader>().levelInstantiationEvent += TryFindAndSetSpawnPoint;
    }

    private void Start() 
    {
        Registry.ins.inputSet.KillPlayerKeyPressEvent += KillPlayer;
    }
    private void OnDestroy()
    {
        Registry.ins.inputSet.KillPlayerKeyPressEvent -= KillPlayer;
        Registry.ins.lm.levelInstantiationEvent -= TryFindAndSetSpawnPoint;
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

        bool canSpawnCorpse = Registry.ins.skullManager.GetSkullsAmount() != 0;
        StartCoroutine(KillPlayerRoutine(canSpawnCorpse));
    }

    private IEnumerator KillPlayerRoutine(bool shouldSpawnCorpse)
    {
        Vector2 vel = _rb.velocity;
        Vector3 pos = player.transform.position;

        Instantiate(smokeEffectPrefab, new Vector3(pos.x, pos.y, smokeEffectPrefab.transform.position.z), Quaternion.identity);

        _rb.constraints = RigidbodyConstraints2D.FreezePosition;
        Registry.ins.inputSet.Active = false;

        yield return new WaitForSeconds(smokeTimeOffset);

        if (shouldSpawnCorpse) 
        {
            Registry.ins.skullManager.DestroySkull();
            DestroyPlayer();
            Transform corpse = Registry.ins.corpseManager.SpawnCorpse(pos, vel).transform;
            corpse.GetComponent<CorpsePhysics>().kickedMode = true;
            Registry.ins.cameraManager.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }

        yield return Registry.ins.cameraManager.TransiteIn();

        if (!shouldSpawnCorpse)
            DestroyPlayer();
        _killingPlayer = false;

        SpawnPlayer();

        yield return Registry.ins.cameraManager.TransiteOut();
        Registry.ins.inputSet.Active = true;
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
        GameObject foundObject = GetComponent<LevelLoader>().TryFindObjectWithTag("SpawnPoint");

        if (foundObject != null)
            SetSpawnPoint(foundObject.transform.position);
        else
            SetSpawnPoint(Vector2.zero);
    }
}
