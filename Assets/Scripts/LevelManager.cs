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

    private Player player;
    private TransitionController transitionController;

    private void Awake()
    {
        Registry.Init();
    }

    void Start()
    {
        Registry.ins.lm = this;

        transitionController = GetComponent<TransitionController>();

        InputSystem.ins.KillPlayerKeyPressEvent += KillPlayer;

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
        player.PlayDeathAnimation();

        yield return transitionController.TransiteIn();

        Registry.ins.skullManager.DestroySkull();
        Destroy(player.gameObject);
        Vector3 pos = player.transform.position;
        player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        Registry.ins.corpseManager.SpawnCorpse(pos, Vector2.zero);

        yield return transitionController.TransiteOut();
    }
#endregion

    #region load

    private IEnumerator UnloadLevelRoutine ()
    {
        yield return transitionController.TransiteIn();

        Registry.ins.corpseManager.ClearCorpses();

        Destroy(player.gameObject);
        Destroy(currentLevel);
    }

    public void LoadLevel (int index)
    {
        StartCoroutine(LoadLevelRoutine(index));
    }

    private IEnumerator LoadLevelRoutine (int index)
    {
        if (currentLevel != null)
        {
            yield return UnloadLevelRoutine();
        }

        respawnPoint = Vector2.zero;
        Registry.ins.skullManager.ClearSkulls();

        currentLevel = Instantiate(levels[index]);
        currentLevelIndex = index;

        player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        
        yield return transitionController.TransiteOut();
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelIndex));
    }
    #endregion

    #region API

    public Player GetPlayer ()
    {
        return player;
    }

    #endregion
}
