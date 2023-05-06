using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    /*
#if UNITY_EDITOR
    public bool loadDirectly;       //if you want to load a level from a plain prefab without a leveltree
    public GameObject levelPrefab;
#endif*/
    
    //public int loadLevelID;         //id of the loading level [LevelData.id]

    [SerializeField] LevelTreeManager levelTreeManager;

    private int currentLevelID;  //index in tree if used
    private LevelTree.LevelData currentLevelData;
    private GameObject currentLevelGameObject;//level in scene
    private GameObject prefabToLoad; //level prefab in files

    private void Awake() => Registry.ins.lm = this;

    void Start()
    {
        /*#if UNITY_EDITOR
                if (loadDirectly)
                    prefabToLoad = levelPrefab;
        #endif*/
        int loadLevelID = PlayerPrefs.GetInt("Level_ID_Selected_in_Menu");
        MakeDecision(loadLevelID);
    }

    public void AttachToLevelAsChild (Transform transform)
    {
        transform.SetParent(currentLevelGameObject.transform);
    }

    public void ProceedFurther ()
    {
        MakeDecision(currentLevelID + 1);
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelData));
    }

    public void QuitToMenu() => SceneManager.LoadScene("Menu");

    private void MakeDecision(int id)
    {
        LevelTree.LevelData? levelData = levelTreeManager.TryGetLevel(id);
        
        if (levelData == null)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
        currentLevelData = levelData.Value;
        currentLevelID = id;
        LoadLevel(levelData.Value);
    }


#region load

    private void PrepareLevelPrefab()
    {
        string path = levelTreeManager.TryGetLevel(currentLevelID).Value.path;
        prefabToLoad = Resources.Load<GameObject>(path);
    }

    private void UnloadLevel ()
    {
        Registry.ins.corpseManager.ClearCorpses();
        Registry.ins.playerManager.DestroyPlayer();
        Destroy(currentLevelGameObject);
    }

    public void LoadLevel (LevelTree.LevelData levelData)
    {
        StartCoroutine(LoadLevelRoutine(levelData));
    }

    private IEnumerator LoadLevelRoutine (LevelTree.LevelData levelData)
    {
        if (currentLevelGameObject != null)
        {
            yield return Registry.ins.cameraManager.TransiteIn();
            UnloadLevel();
        }

        bool allowedToLoadPrefab = true;

/*#if UNITY_EDITOR
        allowedToLoadPrefab = !loadDirectly;
#endif*/
        if (allowedToLoadPrefab)
            PrepareLevelPrefab();

        Registry.ins.skullManager.ClearSkulls();
        currentLevelGameObject = Instantiate(prefabToLoad);

        TryFindAndSetSpawnPoint();
        
        Registry.ins.playerManager.SpawnPlayer();
        yield return Registry.ins.cameraManager.TransiteOut();
    }

    private void TryFindAndSetSpawnPoint ()
    {
        Transform spawnPoint;
        for (int i = 0; i < currentLevelGameObject.transform.childCount; i++)
            if (currentLevelGameObject.transform.GetChild(i).tag == "SpawnPoint")
            {
                spawnPoint = currentLevelGameObject.transform.GetChild(i);
                Registry.ins.playerManager.SetSpawnPoint(spawnPoint.position);
                return;
            }

        Registry.ins.playerManager.SetSpawnPoint(Vector2.zero);
    }
    #endregion

}
/*
#region disgusting unity editor code
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(LevelLoader))]
public class LevelManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        LevelLoader levelManager = (LevelLoader)target;
        
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
#endregion*/