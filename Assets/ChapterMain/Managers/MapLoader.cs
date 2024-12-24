using System.Collections;
using System.Collections.Generic;
using ChapterEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MapLoader : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SequentialSoundPlayer soundPlayer;
    [SerializeField] private string loadedChapterName;
    [SerializeField] private GameObject spawnPointPrefab;
    [SerializeField] private LayerFactory layerFactory;
    
    private GameObject _chapterObject;
    private MapData _currentMapData;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(soundPlayer);
        Assert.IsNotNull(spawnPointPrefab);
        Assert.IsNotNull(layerFactory);
        
        GameSystems.ins.lm = this;
        
        if (!PlayerPrefs.HasKey("Last_Completed_Level_ID"))
            PlayerPrefs.SetInt("Last_Completed_Level_ID", 0);
        
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        soundPlayer.StartPlaying();
        MakeDecision();
    }

    private void OnDestroy() => UnsubscribeFromInput();
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action LevelInstantiationEvent;

    public void AttachToLevelAsChild (Transform transform)
    {
        transform.SetParent(_chapterObject.transform);
    }

    public void ProceedFurther () => MakeDecision();

    //DO NOT CHANGE TO GameObject.FindGameObjectWithTag: IT DOES NOT WORK!
    public GameObject TryFindObjectWithTag(string tag)
    {
        if (_chapterObject == null)
            return null;

        for (int i = 0; i < _chapterObject.transform.childCount; i++)
            if (_chapterObject.transform.GetChild(i).tag == tag)
                return _chapterObject.transform.GetChild(i).gameObject;
        
        return null;
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(_currentMapData));
    }

    private void QuitToMenu()
    {
        StartCoroutine(GoToMenuRoutine());
    }

    private void MakeDecision()
    {
        var loaded = ChapterFileManager.Load(loadedChapterName, out var contents);
        Assert.IsTrue(loaded);
        Assert.IsNotNull(contents);
        
        _currentMapData = new MapData();
        _currentMapData.Unpack(contents);
        StartCoroutine(LoadLevelRoutine(_currentMapData));
    }

    private IEnumerator GoToMenuRoutine ()
    {
        Application.targetFrameRate = -1;

        StartCoroutine(GameSystems.ins.transitionVeil.TransiteIn());
        yield return soundPlayer.StopPlaying();
        yield return new WaitWhile(() => GameSystems.ins.transitionVeil.inTransition);
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LoadLevelRoutine (MapData data)
    {
        UnsubscribeFromInput();

        if (_chapterObject)
        {
            yield return GameSystems.ins.transitionVeil.TransiteIn();

            GameSystems.ins.corpseManager.ClearCorpses();
            GameSystems.ins.playerManager.DestroyPlayer();
            Destroy(_chapterObject);
        }
        GameSystems.ins.fruitManager.ClearFruits();

        var registry = new MapSpaceRegistry(data.SpaceSize);
        for (var i = 0; i < data.LayerNames.Length; i++)
        {
            var manipulator = layerFactory.CreateManipulator(data.LayerNames[i]);
            registry.RegisterObject(manipulator, out _);
            manipulator.Unpack(data.LayerData[i]);
        }

        for (var i = 0; registry.HasLayer(i); i++)
        {
            var manipulator = registry.GetManipulator(i);
            manipulator.KillDrop();
        }
        
        
        LevelInstantiationEvent?.Invoke();
        
        GameSystems.ins.playerManager.SpawnPlayer();
        yield return GameSystems.ins.transitionVeil.TransiteOut();

        SubscribeToInput();
    }
    
    private void SubscribeToInput()
    {
        GameSystems.ins.inputSet.QuitActivationEvent += QuitToMenu;
        GameSystems.ins.inputSet.ReloadActivationEvent += ReloadLevel;
    }

    private void UnsubscribeFromInput()
    {
        GameSystems.ins.inputSet.QuitActivationEvent -= QuitToMenu;
        GameSystems.ins.inputSet.ReloadActivationEvent -= ReloadLevel;
    }
}