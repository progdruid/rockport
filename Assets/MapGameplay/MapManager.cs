using System.Collections;
using MapEditor;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private string loadedChapterName;
    [SerializeField] private SequentialSoundPlayer soundPlayer;
    [SerializeField] private EntityFactory entityFactory;
    [SerializeField] private ParallaxBackground[] parallaxBackgrounds;
    
    private JSONNode _currentMapData;
    private MapSpace _currentMapSpace;
    
    private bool _isLoading = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(soundPlayer);
        Assert.IsNotNull(entityFactory);
        
        GameSystems.Ins.MapManager = this;
        
        Application.targetFrameRate = 60;
        
        Assert.IsTrue(PlayerPrefs.HasKey("LoadedMap"));
    }

    private void Start()
    {
        soundPlayer.StartPlaying();
        if (PlayerPrefs.HasKey("LoadedMap")) 
            loadedChapterName = PlayerPrefs.GetString("LoadedMap");
        LoadMap(loadedChapterName);
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void ReloadMap()
    {
        if (!_isLoading)
            StartCoroutine(LoadMapRoutine(_currentMapData));
    }

    public void QuitToScene(string sceneName)
    {
        if (!_isLoading)
            routine().Start(this);
        return;
        
        IEnumerator routine()
        {
            Application.targetFrameRate = -1;

            StartCoroutine(GameSystems.Ins.TransitionVeil.TransiteIn());
            yield return soundPlayer.StopPlaying();
            yield return new WaitWhile(() => GameSystems.Ins.TransitionVeil.inTransition);
            PlayerPrefs.SetString("LoadedMap", loadedChapterName);
            SceneManager.LoadScene(sceneName);
        }
    }
    
    public void LoadMap(string mapName)
    {
        if (_isLoading)
            return;
        
        var loaded = MapSaveManager.Load(mapName, out var contents);
        Assert.IsTrue(loaded);
        Assert.IsNotNull(contents);

        loadedChapterName = mapName;
        _currentMapData = contents;
        StartCoroutine(LoadMapRoutine(_currentMapData));
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator LoadMapRoutine (JSONNode mapData)
    {
        _isLoading = true;

        if (_currentMapSpace != null)
        {
            GameSystems.Ins.Controller.SetAllowMove(false);
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

        
        for (var i = 0; mapSpace.HasLayer(i); i++)
        {
            var entity = mapSpace.GetEntity(i);
            entity.Activate();
        }
        signalCircuit.Replicate(mapData["signalData"].AsObject);;
        _currentMapSpace = mapSpace;
        

        
        GameSystems.Ins.PlayerManager.SpawnPlayer();
        
        foreach (var background in parallaxBackgrounds)
            background.SetTarget(Camera.main.transform);
        mapSpace.FindEntity(GlobalConfig.Ins.groundMarkerEntityName, out var foundGroundMarker);
        if (foundGroundMarker)
            foreach (var background in parallaxBackgrounds)
                background.SetGroundAnchor(foundGroundMarker.Target.position.To2());

        yield return GameSystems.Ins.TransitionVeil.TransiteOut();
        GameSystems.Ins.Controller.SetAllowMove(true);
        _isLoading = false;
    }
}