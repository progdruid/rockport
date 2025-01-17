using System.Collections;
using MapEditor;
using Map;
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
    
    private MapData _currentMapData;
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
        var loaded = MapSaveManager.Load(loadedChapterName, out var contents);
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

        var mapSpace = new MapSpace(data.SpaceSize);
        var signalCircuit = new SignalCircuit();
        
        for (var i = 0; i < data.LayerNames.Length; i++)
        {
            var entity = entityFactory.CreateEntity(data.LayerNames[i]);
            mapSpace.RegisterObject(entity, out _);
            entity.Unpack(data.LayerData[i]);
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
        
        signalCircuit.Unpack(data.SignalData);

        _currentMapSpace = mapSpace;
        LevelInstantiationEvent?.Invoke();
        
        GameSystems.Ins.PlayerManager.SpawnPlayer();
        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        controller.AllowMove = true;

        _isLoading = false;
    }
}