using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] LevelTreeManager levelTreeManager;
    [SerializeField] SequentialSoundPlayer soundPlayer;

    public event System.Action levelInstantiationEvent = delegate { };

    private int currentLevelID;  //index in tree if used
    private LevelTree.LevelData currentLevelData;
    private GameObject currentLevelGameObject;//level in scene


    public void AttachToLevelAsChild (Transform transform)
    {
        transform.SetParent(currentLevelGameObject.transform);
    }

    public void SetCurrentLevelAsCompleted ()
    {
        int lastCompletedLevelID = PlayerPrefs.GetInt("Last_Completed_Level_ID");
        if (lastCompletedLevelID < currentLevelID)
            PlayerPrefs.SetInt("Last_Completed_Level_ID", lastCompletedLevelID + 1);
    }

    public void ProceedFurther ()
    {
        SetCurrentLevelAsCompleted ();
        MakeDecision(currentLevelID + 1);
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelData));
    }

    public void QuitToMenu()
    {
        StartCoroutine(GoToMenuRoutine());
    }

    //DO NOT CHANGE TO GameObject.FindGameObjectWithTag: IT DOES NOT WORK!
    public GameObject TryFindObjectWithTag(string tag)
    {
        if (currentLevelGameObject == null)
            return null;

        for (int i = 0; i < currentLevelGameObject.transform.childCount; i++)
            if (currentLevelGameObject.transform.GetChild(i).tag == tag)
                return currentLevelGameObject.transform.GetChild(i).gameObject;
        
        return null;
    }

    private void MakeDecision(int id)
    {
        LevelTree.LevelData levelData = levelTreeManager.TryGetLevel(id);
        
        if (levelData == null)
        {
            StartCoroutine(GoToMenuRoutine());
            return;
        }
        currentLevelData = levelData;
        currentLevelID = id;
        StartCoroutine(LoadLevelRoutine(levelData));
    }

    private IEnumerator GoToMenuRoutine ()
    {
        Application.targetFrameRate = -1;

        StartCoroutine(Registry.ins.transitionVeil.TransiteIn());
        yield return soundPlayer.StopPlaying();
        yield return new WaitWhile(() => Registry.ins.transitionVeil.inTransition);
        SceneManager.LoadScene("Menu");
    }
    
    private GameObject GetLevelPrefabFromFiles()
    {
        string path = levelTreeManager.TryGetLevel(currentLevelID).path;
        return Resources.Load<GameObject>(path);
    }

    private IEnumerator LoadLevelRoutine (LevelTree.LevelData levelData)
    {
        UnsubscribeFromInput();

        if (currentLevelGameObject != null)
        {
            yield return Registry.ins.transitionVeil.TransiteIn();

            Registry.ins.corpseManager.ClearCorpses();
            Registry.ins.playerManager.DestroyPlayer();
            Destroy(currentLevelGameObject);
        }

        GameObject prefabToLoad = GetLevelPrefabFromFiles();

        Registry.ins.fruitManager.ClearFruits();
        currentLevelGameObject = Instantiate(prefabToLoad);
        
        levelInstantiationEvent();
        
        Registry.ins.playerManager.SpawnPlayer();
        yield return Registry.ins.transitionVeil.TransiteOut();

        SubscribeToInput();
    }

    private void SubscribeToInput()
    {
        Registry.ins.inputSet.QuitActivationEvent += QuitToMenu;
        Registry.ins.inputSet.ReloadActivationEvent += ReloadLevel;
    }

    private void UnsubscribeFromInput()
    {
        Registry.ins.inputSet.QuitActivationEvent -= QuitToMenu;
        Registry.ins.inputSet.ReloadActivationEvent -= ReloadLevel;
    }

    private void Awake() => Registry.ins.lm = this;
    void Start()
    {
        if (!PlayerPrefs.HasKey("Last_Completed_Level_ID"))
            PlayerPrefs.SetInt("Last_Completed_Level_ID", 0);

        //PlayerPrefs.SetInt("Last_Completed_Level_ID", 16);

        Application.targetFrameRate = 60;

        int loadLevelID = PlayerPrefs.GetInt("Level_ID_Selected_in_Menu");
        soundPlayer.StartPlaying();
        MakeDecision(loadLevelID);
    }
    private void OnDestroy() => UnsubscribeFromInput();
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