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

    private void OnEnable() => Registry.ins.playerManager = this;
    private void Start()
    {
        Registry.ins.inputSystem.KillPlayerKeyPressEvent += KillPlayer;
    }
    private void OnDestroy()
    {
        Registry.ins.inputSystem.KillPlayerKeyPressEvent -= KillPlayer;
    }

    public void SetSpawnPoint (Vector2 pos)
    {
        spawnPoint = pos;
    }

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
        player.PlayDeathAnimation();

        if (Registry.ins.skullManager.GetSkullsAmount() == 0)
        {
            Registry.ins.lm.ReloadLevel();
        }
        else
        {
            StartCoroutine(KillPlayerRoutine());
        }
    }

    private IEnumerator KillPlayerRoutine()
    {
        Vector2 vel = rb.velocity;
        Vector3 pos = player.transform.position;
        yield return Registry.ins.tc.TransiteIn();

        Registry.ins.skullManager.DestroySkull();
        DestroyPlayer();
        SpawnPlayer();
        Registry.ins.corpseManager.SpawnCorpse(pos, vel);

        yield return Registry.ins.tc.TransiteOut();
    }

    public void DestroyPlayer ()
    {
        Destroy(player.gameObject);
        player = null;
    }
}
