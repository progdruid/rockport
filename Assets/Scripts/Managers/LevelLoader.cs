using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] LevelTreeManager levelTreeManager;

    public event System.Action levelInstantiationEvent = delegate { };

    private int currentLevelID;  //index in tree if used
    private LevelTree.LevelData currentLevelData;
    private GameObject currentLevelGameObject;//level in scene

    //private int defaultVSyncCount = 0;

    private void Awake() => Registry.ins.lm = this;

    void Start()
    {
        Application.targetFrameRate = 60;
        //defaultVSyncCount = QualitySettings.vSyncCount;
        //QualitySettings.vSyncCount = 2;
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

    public void QuitToMenu()
    {
        Application.targetFrameRate = -1;
        //QualitySettings.vSyncCount = defaultVSyncCount;

        SceneManager.LoadScene("Menu");
    }
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

    private GameObject GetLevelPrefab()
    {
        string path = levelTreeManager.TryGetLevel(currentLevelID).Value.path;
        return Resources.Load<GameObject>(path);
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

        GameObject prefabToLoad = GetLevelPrefab();

        Registry.ins.skullManager.ClearSkulls();
        currentLevelGameObject = Instantiate(prefabToLoad);
        
        levelInstantiationEvent();
        
        Registry.ins.playerManager.SpawnPlayer();
        yield return Registry.ins.cameraManager.TransiteOut();
    }
    #endregion

    //DO NOT CHANGE TO GameObject.FindGameObjectWithTag: IT DOES NOT WORK!
    public GameObject TryFindObjectWithTag(string tag)
    {
        for (int i = 0; i < currentLevelGameObject.transform.childCount; i++)
            if (currentLevelGameObject.transform.GetChild(i).tag == tag)
                return currentLevelGameObject.transform.GetChild(i).gameObject;
        
        return null;
    }

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