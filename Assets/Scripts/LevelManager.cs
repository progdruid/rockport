using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int levelIndex;
    public GameObject[] levels;

    private int currentLevelIndex;
    private GameObject currentLevel;

    private void OnEnable() => Registry.ins.lm = this;

    void Start()
    {
        LoadLevel(levelIndex);
        currentLevelIndex = levelIndex;
    }

    #region load

    private IEnumerator UnloadLevelRoutine ()
    {
        yield return Registry.ins.tc.TransiteIn();

        Registry.ins.corpseManager.ClearCorpses();

        Registry.ins.playerManager.DestroyPlayer();
        Destroy(currentLevel);
    }

    public void LoadLevel (int index)
    {
        Registry.ins.playerManager.SetSpawnPoint(Vector2.zero);
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

        Registry.ins.playerManager.SpawnPlayer();
        
        yield return Registry.ins.tc.TransiteOut();
    }
    #endregion
}
