using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int loadLevelID;
    public string leveltreePath;

    private int currentLevelIndex;
    private GameObject currentLevel;

    private LevelTree levelTree;

    private void OnEnable() => Registry.ins.lm = this;

    void Start()
    {
        levelTree = LevelTree.Extract(leveltreePath);
        currentLevelIndex = levelTree.GetLevelIndex(loadLevelID);

        LoadLevel(currentLevelIndex);
    }

    #region load

    private IEnumerator UnloadLevelRoutine ()
    {
        yield return Registry.ins.cameraManager.TransiteIn();

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

        string path = levelTree.levels[index].path;
        GameObject prefab = Resources.Load<GameObject>(path);
        currentLevel = Instantiate(prefab);
        currentLevelIndex = index;

        Registry.ins.playerManager.SpawnPlayer();
        
        yield return Registry.ins.cameraManager.TransiteOut();
    }
    #endregion
}
