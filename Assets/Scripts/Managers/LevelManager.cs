using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
#if UNITY_EDITOR
    public bool loadDirectly;
    public GameObject levelPrefab;
#endif

    public int loadLevelID;
    public string leveltreePath;

    private GameObject levelToLoad;
    private int currentLevelIndex;
    private GameObject currentLevel;

    private LevelTree levelTree;

    private void OnEnable() => Registry.ins.lm = this;

    void Start()
    {
#if UNITY_EDITOR
        if (loadDirectly)
            levelToLoad = levelPrefab;
        else
            LoadTree();
#endif

#if !UNITY_EDITOR
        LoadTree();
#endif
        LoadLevel(currentLevelIndex);
    }

    private void LoadTree ()
    {
        levelTree = LevelTree.Extract(leveltreePath);
        currentLevelIndex = levelTree.GetLevelIndex(loadLevelID);

        string path = levelTree.levels[currentLevelIndex].path;
        levelToLoad = Resources.Load<GameObject>(path);
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

        currentLevel = Instantiate(levelToLoad);
        currentLevelIndex = index;

        Registry.ins.playerManager.SpawnPlayer();
        
        yield return Registry.ins.cameraManager.TransiteOut();
    }
#endregion
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        LevelManager levelManager = (LevelManager)target;
        
        levelManager.loadDirectly = UnityEditor.EditorGUILayout.Toggle("Load Directly", levelManager.loadDirectly);
        
        UnityEditor.EditorGUILayout.Space();

        if (levelManager.loadDirectly)
        {
            levelManager.levelPrefab = (GameObject)UnityEditor.EditorGUILayout.ObjectField("Load Prefab", levelManager.levelPrefab, typeof(GameObject), true);
        }
        else
        {
            levelManager.loadLevelID = UnityEditor.EditorGUILayout.IntField("Load Level ID", levelManager.loadLevelID);
            levelManager.leveltreePath = UnityEditor.EditorGUILayout.TextField("Leveltree Path", levelManager.leveltreePath);
        }
    }
}
#endif