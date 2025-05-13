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
    [SerializeField] private ParallaxBackground[] parallaxBackgrounds;
    
    private JSONNode _currentMapData;
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
        if (PlayerPrefs.HasKey("TestMap")) 
            loadedChapterName = PlayerPrefs.GetString("TestMap");
        MakeDecision();
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action LevelInstantiationEvent;
    
    public void ProceedFurther (string mapName)
    {
        if (_isLoading) return;
        loadedChapterName = mapName;
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
        var loaded = MapSaveManager.Load(loadedChapterName, out var contents);
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

    private IEnumerator LoadLevelRoutine (JSONNode mapData)
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
        foreach (var background in parallaxBackgrounds)
            background.Clear();
        GameSystems.Ins.FruitManager.ClearFruits();

        
        var mapSpace = new MapSpace(mapData["spaceSize"].ReadVector2Int());
        var signalCircuit = new SignalCircuit();
        var layers = mapData["layers"].AsArray;
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i].AsObject;
            var entity = GlobalConfig.Ins.entityFactory.CreateEntity(layer["title"].Value);
            mapSpace.RegisterAt(entity, i);
            entity.Initialise();
            entity.Replicate(layer);
            signalCircuit.ExtractAndAdd(entity);
        }
        
        
        mapSpace.FindEntity(GlobalConfig.Ins.spawnPointEntityName, out var foundSpawnPoint);
        Assert.IsNotNull(foundSpawnPoint);
        var spawnPos = foundSpawnPoint.Target.position.To2();
        var spawnZ = foundSpawnPoint.GetReferenceZ();
        
        GameSystems.Ins.PlayerManager.SetSpawnPoint(spawnPos);
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
        
        foreach (var background in parallaxBackgrounds)
            background.SetTarget(Camera.main.transform);
        mapSpace.FindEntity(GlobalConfig.Ins.groundMarkerEntityName, out var foundGroundMarker);
        if (foundGroundMarker)
            foreach (var background in parallaxBackgrounds)
                background.SetGroundLevel(foundGroundMarker.Target.position.y);

        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        controller.AllowMove = true;
        _isLoading = false;
    }
}