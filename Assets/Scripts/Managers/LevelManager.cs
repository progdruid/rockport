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

    //temporary property
    //to-do in future:
    //  * rename LevelManager -> LevelLoader which will only accept LevelData as an input
    //  * create a separate class LevelTreeManager or smth that will manage all processes with the level tree
    public LevelTree levelTree { get; private set; } 

    private int currentLevelIndex;  //index in tree if used
    private GameObject currentLevel;//level in scene
    private GameObject levelToLoad; //level prefab in files

    private void Awake() => Registry.ins.lm = this;

    void Start()
    {
        bool allowedToExtractLevelTree = true;
        //GameObject.FindGameObjectWithTag("Test").SetActive(false);
#if UNITY_EDITOR
        allowedToExtractLevelTree = false;
        //GameObject.FindGameObjectWithTag("Test").SetActive(false);
        if (loadDirectly)
            levelToLoad = levelPrefab;
        else
        {
            levelTree = LevelTree.Extract(leveltreePath);
            //GameObject.FindGameObjectWithTag("Test").SetActive(false);
        }
#endif
        //GameObject.FindGameObjectWithTag("Test").SetActive(false);
        if (allowedToExtractLevelTree)
        {
            //GameObject.FindGameObjectWithTag("Test").SetActive(false);
            levelTree = LevelTree.Extract(leveltreePath); 
            //GameObject.FindGameObjectWithTag("Test").SetActive(false);
        }
        //GameObject.FindGameObjectWithTag("Test").SetActive(false);
        LoadLevel();
    }

    public void AttachToLevelAsChild (Transform transform)
    {
        transform.SetParent(currentLevel.transform);
    }

#region load

    private void LoadAndPrepareLevelPrefab()
    {
        currentLevelIndex = levelTree.GetLevelIndex(loadLevelID);

        string path = levelTree.levels[currentLevelIndex].path;
        levelToLoad = Resources.Load<GameObject>(path);
    }

    private void UnloadLevel ()
    {
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
            yield return Registry.ins.cameraManager.TransiteIn();
            UnloadLevel();
        }

        bool allowedToLoadPrefab = true;
#if UNITY_EDITOR
        allowedToLoadPrefab = !loadDirectly;
#endif
        if (allowedToLoadPrefab)
            LoadAndPrepareLevelPrefab();

        Registry.ins.skullManager.ClearSkulls();
        currentLevel = Instantiate(levelToLoad);

        TryFindAndSetSpawnPoint();
        
        Registry.ins.playerManager.SpawnPlayer();
        yield return Registry.ins.cameraManager.TransiteOut();
    }

    private void TryFindAndSetSpawnPoint ()
    {
        Transform spawnPoint;
        for (int i = 0; i < currentLevel.transform.childCount; i++)
            if (currentLevel.transform.GetChild(i).tag == "SpawnPoint")
            {
                spawnPoint = currentLevel.transform.GetChild(i);
                Registry.ins.playerManager.SetSpawnPoint(spawnPoint.position);
                return;
            }

        Registry.ins.playerManager.SetSpawnPoint(Vector2.zero);
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