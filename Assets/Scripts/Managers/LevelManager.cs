using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
#if UNITY_EDITOR
    public bool loadDirectly;       //if you want to load a level from a plain prefab without a leveltree
    public GameObject levelPrefab; 
#endif

    public int loadLevelID;         //id of the loading level [LevelData.id]
    public string leveltreePath;

    private int currentLevelIndex;  //index in tree if used
    private GameObject currentLevel;//level in scene
    private GameObject levelToLoad; //level prefab in files

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
        LoadLevel();
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

    public void LoadLevel ()
    {
        StartCoroutine(LoadLevelRoutine());
    }

    private IEnumerator LoadLevelRoutine ()
    {
        if (currentLevel != null)
        {
            yield return UnloadLevelRoutine();
        }

        Registry.ins.skullManager.ClearSkulls();
        currentLevel = Instantiate(levelToLoad);

        TryFindAndSetSpawnPoint();
        
        Registry.ins.playerManager.SpawnPlayer();
        yield return Registry.ins.cameraManager.TransiteOut();
    }

    private void TryFindAndSetSpawnPoint ()
    {
        try
        {
            Transform spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
            Registry.ins.playerManager.SetSpawnPoint(spawnPoint.position);
        }
        catch
        { 
            Registry.ins.playerManager.SetSpawnPoint(Vector2.zero); 
        }
    }
    #endregion
}

#region disgusting unity editor code
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
#endregion