using System.Collections;
using ChapterEditor;
using Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MapLoader : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SequentialSoundPlayer soundPlayer;
    [SerializeField] private string loadedChapterName;
    [SerializeField] private EntityFactory entityFactory;
    
    private GameObject _chapterObject;
    private MapData _currentMapData;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(soundPlayer);
        Assert.IsNotNull(entityFactory);
        
        GameSystems.Ins.Loader = this;
        
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

    public void AttachToLevelAsChild (Transform transform) => transform.SetParent(_chapterObject.transform);
    public void ProceedFurther () => MakeDecision();

    //DO NOT CHANGE TO GameObject.FindGameObjectWithTag: IT DOES NOT WORK!
    public GameObject TryFindObjectWithTag(string tag)
    {
        if (!_chapterObject)
            return null;

        for (var i = 0; i < _chapterObject.transform.childCount; i++)
            if (_chapterObject.transform.GetChild(i).CompareTag(tag))
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

        StartCoroutine(GameSystems.Ins.TransitionVeil.TransiteIn());
        yield return soundPlayer.StopPlaying();
        yield return new WaitWhile(() => GameSystems.Ins.TransitionVeil.inTransition);
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator LoadLevelRoutine (MapData data)
    {
        UnsubscribeFromInput();

        if (_chapterObject)
        {
            yield return GameSystems.Ins.TransitionVeil.TransiteIn();

            GameSystems.Ins.CorpseManager.ClearCorpses();
            GameSystems.Ins.PlayerManager.DestroyPlayer();
            Destroy(_chapterObject);
        }
        GameSystems.Ins.FruitManager.ClearFruits();

        var mapSpace = new MapSpace(data.SpaceSize);
        
        for (var i = 0; i < data.LayerNames.Length; i++)
        {
            var entity = entityFactory.CreateEntity(data.LayerNames[i]);
            mapSpace.RegisterObject(entity, out _);
            entity.Unpack(data.LayerData[i]);
        }

        GameSystems.Ins.CameraManager.ObservationHeight = mapSpace.GetTopmostEntity().GetReferenceZ();

        mapSpace.FindEntity(GlobalConfig.Ins.spawnPointEntityName, out var foundEntity);
        Assert.IsNotNull(foundEntity);
        var spawnPoint = foundEntity as AnchorEntity;
        Assert.IsNotNull(spawnPoint);
        var spawnPos = spawnPoint.GetPos();
        var spawnZ = spawnPoint.GetReferenceZ();

        for (var i = 0; mapSpace.HasLayer(i); i++)
        {
            var entity = mapSpace.GetEntity(i);
            entity.Activate();
        }
        
        GameSystems.Ins.PlayerManager.SetSpawnPoint(mapSpace.ConvertMapToWorld(spawnPos));
        GameSystems.Ins.PlayerManager.SetSpawnZ(spawnZ);
        
        LevelInstantiationEvent?.Invoke();
        
        GameSystems.Ins.PlayerManager.SpawnPlayer();
        yield return GameSystems.Ins.TransitionVeil.TransiteOut();

        SubscribeToInput();
    }
    
    private void SubscribeToInput()
    {
        GameSystems.Ins.InputSet.QuitActivationEvent += QuitToMenu;
        GameSystems.Ins.InputSet.ReloadActivationEvent += ReloadLevel;
    }

    private void UnsubscribeFromInput()
    {
        GameSystems.Ins.InputSet.QuitActivationEvent -= QuitToMenu;
        GameSystems.Ins.InputSet.ReloadActivationEvent -= ReloadLevel;
    }
}