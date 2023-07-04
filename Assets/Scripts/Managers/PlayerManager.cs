using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    public event System.Action<GameObject> PlayerSpawnEvent = delegate { };

    public Player player { get; private set; }
    private Rigidbody2D rb => player.GetComponent<Rigidbody2D>();
    private Vector2 spawnPoint = Vector2.zero;
    private bool killingPlayer;

    private void Awake()
    {
        Registry.ins.playerManager = this;
        GetComponent<LevelLoader>().levelInstantiationEvent += TryFindAndSetSpawnPoint;
    }

    private void Start() 
    {
        Registry.ins.inputSystem.KillPlayerKeyPressEvent += KillPlayer;
        //Registry.ins.lm.levelInstantiationEvent += TryFindAndSetSpawnPoint;
    }
    private void OnDestroy()
    {
        Registry.ins.inputSystem.KillPlayerKeyPressEvent -= KillPlayer;
        Registry.ins.lm.levelInstantiationEvent -= TryFindAndSetSpawnPoint;
    }
    public void SetSpawnPoint (Vector2 pos) => spawnPoint = pos;
    
    public void SpawnPlayer ()
    {
        if (player != null)
        {
            #if UNITY_EDITOR 
            Debug.LogError("There is already one instance of player.");
            #endif
            
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(spawnPoint.x, spawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        PlayerSpawnEvent(player.gameObject);
    }

    public void KillPlayer ()
    {
        if (killingPlayer)
            return;
        killingPlayer = true;
        player.PlayDeathAnimation();

        bool spawnCorpse = Registry.ins.skullManager.GetSkullsAmount() != 0;
        StartCoroutine(KillPlayerRoutine(spawnCorpse));
    }

    private IEnumerator KillPlayerRoutine(bool spawnCorpse)
    {
        Vector2 vel = rb.velocity;
        Vector3 pos = player.transform.position;
        
        if (spawnCorpse) 
        {
            Registry.ins.skullManager.DestroySkull();
            DestroyPlayer();
            Transform corpse = Registry.ins.corpseManager.SpawnCorpse(pos, vel).transform;
            corpse.GetComponent<CorpsePhysics>().kickedMode = true;
            Registry.ins.cameraManager.SetTarget(corpse);
            yield return new WaitForSeconds(0.5f);
        }

        yield return Registry.ins.cameraManager.TransiteIn();

        if (!spawnCorpse)
            DestroyPlayer();
        killingPlayer = false;

        SpawnPlayer();

        yield return Registry.ins.cameraManager.TransiteOut();
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
        Debug.Log(spawnPoint);
    }
}
