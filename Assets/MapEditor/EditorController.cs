
using System;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Map;
using SimpleJSON;

namespace MapEditor
{

public class EditorController : MonoBehaviour, IReplicable
{
    /// TODO: should be extracted to a separate UI system
    public static bool CanEdit = true;

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Vector2Int initMapSize;
    [Space] 
    [SerializeField] private EntityEditor entityEditor;
    [SerializeField] private WiringEditor wiringEditor;
    [Space] 
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float cameraMinSize;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;

    private IMapEditorMode[] _editorModes;
    private int _currentModeIndex;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(entityEditor);
        Assert.IsNotNull(wiringEditor);
        Assert.IsNotNull(cam);
        
        _map = new MapSpace(initMapSize);
        _signalCircuit = new SignalCircuit();

        entityEditor.Inject(_map);
        entityEditor.Inject(_signalCircuit);
        
        wiringEditor.Inject(_map);
        wiringEditor.Inject(_signalCircuit);
        
        _editorModes = new IMapEditorMode[]
        {
            entityEditor,
            wiringEditor,
        };
        _currentModeIndex = 0;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public JSONNode ExtractData()
    {
        var mapData = new JSONObject();
        mapData["spaceSize"]= _map.MapSize.ToJson();
        mapData["signalData"] = _signalCircuit.ExtractData();
        var layers = new JSONArray();
        
        for (var i = 0; i < _map.EntitiesCount; i++)
        {
            var entity = _map.GetEntity(i);
            var entityData = entity.ExtractData();
            layers.Add(entityData);
        }
        
        mapData["layers"] = layers;

        //don't beat me, just checkin' if the packing was successful
        Assert.IsTrue(new Func<bool>((() =>
        {
            try
            {
                Replicate(mapData);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        })).Invoke());

        return mapData;
    }

    public void Replicate(JSONNode mapData)
    {
        _editorModes[_currentModeIndex].Exit();

        while (_map.EntitiesCount > 0)
        {
            var dead = _map.GetEntity(0);
            _signalCircuit.ExtractAndRemove(dead);
            _map.UnregisterAt(0);
            dead.Clear();
        }
        _map.Kill();

        
        _map = new MapSpace(mapData["spaceSize"].ReadVector2Int());
        
        var layers = mapData["layers"].AsArray;
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i].AsObject;
            var entity = GlobalConfig.Ins.entityFactory.CreateEntity(layer["title"]);
            _map.RegisterAt(entity, i);
            entity.Initialise();
            entity.Replicate(layer);
            _signalCircuit.ExtractAndAdd(entity);
        }

        _signalCircuit.Replicate(mapData["signalData"].AsObject);

        entityEditor.Inject(_map);
        wiringEditor.Inject(_map);
        _editorModes[_currentModeIndex].Enter();
    }

    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!CanEdit) return;
        
        // camera
        var horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        var vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        var zoom = -Input.mouseScrollDelta.y;

        var worldSize = _map.WorldSize;
        
        var prevMouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        cam.orthographicSize += zoom * cameraZoomSpeed * cam.orthographicSize * Time.deltaTime;
        cam.orthographicSize = cam.orthographicSize
            .ClampBottom(cameraMinSize)
            .ClampTop(worldSize.y)
            .ClampTop(worldSize.x / cam.aspect);

        var newMouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        var worldOffset = prevMouseWorldPos - newMouseWorldPos;
        cam.transform.position += worldOffset;

        var halfHeight = cam.orthographicSize;

        var worldStart = _map.WorldStart;
        var worldEnd = worldStart + worldSize;

        var x = (cam.transform.position.x + horizontal * halfHeight * cameraMoveSpeed * Time.deltaTime)
            .ClampBottom(worldStart.x)
            .ClampTop(worldEnd.x);

        var y = (cam.transform.position.y + vertical * halfHeight * cameraMoveSpeed * Time.deltaTime)
            .ClampBottom(worldStart.y)
            .ClampTop(worldEnd.y);
        
        //updating camera position, so it is always behind the topmost layer
        cam.transform.SetWorld(x, y, _map.GetMapTop() - 5);
        
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _editorModes[_currentModeIndex].Exit();
            _currentModeIndex = (_currentModeIndex + 1) % _editorModes.Length;
            _editorModes[_currentModeIndex].Enter();
        }
        
        // mouse pos in world
        var mousePos = Input.mousePosition;
        var ray = cam.ScreenPointToRay(mousePos);

        var t = - ray.origin.z / ray.direction.z;
        var worldPos = (ray.origin + ray.direction * t);
        
        _editorModes[_currentModeIndex].HandleInput(worldPos);
    }
}

}