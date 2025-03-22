using System.Collections;
using MapEditor;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MapLoader : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private string loadedChapterName;
    [SerializeField] private SequentialSoundPlayer soundPlayer;
    [SerializeField] private EntityFactory entityFactory;
    [SerializeField] private GameplayController controller;
    
    private JSONObject _currentMapData;
    private MapSpace _currentMapSpace;
    
    private bool _isLoading = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(soundPlayer);
        Assert.IsNotNull(entityFactory);
        Assert.IsNotNull(controller);
        
        GameSystems.Ins.Loader = this;
        
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        soundPlayer.StartPlaying();
        MakeDecision();
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action LevelInstantiationEvent;
    
    public void ProceedFurther ()
    {
        if (!_isLoading)
            MakeDecision();
    }

    public void ReloadLevel()
    {
        if (!_isLoading)
            StartCoroutine(LoadLevelRoutine(_currentMapData));
    }

    public void QuitToMenu()
    {
        if (!_isLoading)
            StartCoroutine(GoToMenuRoutine());
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void MakeDecision()
    {
        var nameToLoad = loadedChapterName;
        if (PlayerPrefs.HasKey("TestMap")) 
            nameToLoad = PlayerPrefs.GetString("TestMap");
        
        var loaded = MapSaveManager.Load(nameToLoad, out var contents);
        Assert.IsTrue(loaded);
        Assert.IsNotNull(contents);

        _currentMapData = contents;
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

    private IEnumerator LoadLevelRoutine (JSONObject mapData)
    {
        _isLoading = true;

        if (_currentMapSpace != null)
        {
            controller.AllowMove = false;
            yield return GameSystems.Ins.TransitionVeil.TransiteIn();

            GameSystems.Ins.CorpseManager.ClearCorpses();
            GameSystems.Ins.PlayerManager.DestroyPlayer();
            _currentMapSpace.Kill();
            _currentMapSpace = null;
        }
        GameSystems.Ins.FruitManager.ClearFruits();

        var mapSpace = new MapSpace(mapData["spaceSize"].ReadVector2Int());
        var signalCircuit = new SignalCircuit();
        
        var layers = mapData["layers"].AsArray;
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i].AsObject;
            var entity = GlobalConfig.Ins.entityFactory.CreateEntity(layer["title"].Value);
            mapSpace.RegisterAt(entity, i);
            entity.Replicate(layer["data"].AsObject);
            signalCircuit.ExtractAndAdd(entity);
        }

        mapSpace.FindEntity(GlobalConfig.Ins.spawnPointEntityName, out var foundEntity);
        Assert.IsNotNull(foundEntity);
        var spawnPoint = foundEntity as AnchorEntity;
        Assert.IsNotNull(spawnPoint);
        var spawnPos = spawnPoint.GetPos();
        var spawnZ = spawnPoint.GetReferenceZ();
        
        GameSystems.Ins.PlayerManager.SetSpawnPoint(mapSpace.ConvertMapToWorld(spawnPos));
        GameSystems.Ins.PlayerManager.SetSpawnZ(spawnZ);

        GameSystems.Ins.GameplayCamera.ObservationHeight = mapSpace.GetMapTop();

        for (var i = 0; mapSpace.HasLayer(i); i++)
        {
            var entity = mapSpace.GetEntity(i);
            entity.Activate();
        }
        
        signalCircuit.Replicate(mapData["signalData"].AsObject);;

        _currentMapSpace = mapSpace;
        LevelInstantiationEvent?.Invoke();
        
        GameSystems.Ins.PlayerManager.SpawnPlayer();
        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        controller.AllowMove = true;

        _isLoading = false;
    }
}