using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public int levelIndex;
    public GameObject[] levels;

    private int currentLevelIndex;
    private GameObject currentLevel;
    [HideInInspector]
    public Vector2 respawnPoint;

    private void Awake()
    {
        Registry.Init();
    }

    void Start()
    {
        Registry.ins.lm = this;

        Registry.ins.inputSystem.KillPlayerKeyPressEvent += KillPlayer;

        LoadLevel(levelIndex);
        currentLevelIndex = levelIndex;
    }

    #region kill
    public void KillPlayer()
    {
        if (Registry.ins.skullManager.GetSkullsAmount() == 0)
        {
            ReloadLevel();
        }
        else
        {
            StartCoroutine(KillPlayerRoutine());
        }
    }

    private IEnumerator KillPlayerRoutine()
    {
        Registry.ins.player.PlayDeathAnimation();

        yield return Registry.ins.tc.TransiteIn();

        Registry.ins.skullManager.DestroySkull();
        Destroy(Registry.ins.player.gameObject);
        Vector3 pos = Registry.ins.player.transform.position;
        Registry.ins.player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        Registry.ins.corpseManager.SpawnCorpse(pos, Vector2.zero);

        yield return Registry.ins.tc.TransiteOut();
    }
#endregion

    #region load

    private IEnumerator UnloadLevelRoutine ()
    {
        yield return Registry.ins.tc.TransiteIn();

        Registry.ins.corpseManager.ClearCorpses();

        Destroy(Registry.ins.player.gameObject);
        Destroy(currentLevel);
    }

    public void LoadLevel (int index)
    {
        respawnPoint = Vector2.zero;
        StartCoroutine(LoadLevelRoutine(index));
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelIndex));
    }

    private IEnumerator LoadLevelRoutine (int index)
    {
        if (currentLevel != null)
        {
            yield return UnloadLevelRoutine();
        }

        Registry.ins.skullManager.ClearSkulls();

        currentLevel = Instantiate(levels[index]);
        currentLevelIndex = index;

        Registry.ins.player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        
        yield return Registry.ins.tc.TransiteOut();
    }
    #endregion
}
