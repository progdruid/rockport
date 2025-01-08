
using UnityEngine;
using UnityEngine.Assertions;
using Map;

namespace MapEditor
{

public class EditorController : MonoBehaviour, IPackable
{
    /// TODO: should be extracted to a separate UI system
    public static bool CanEdit = true;

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Vector2Int initMapSize;
    [Space] 
    [SerializeField] private EntityEditor entityEditor;
    [Space] 
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float cameraMinSize;
    [SerializeField] private float cameraRelativeClearance;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;

    private IMapEditorMode[] _editorModes;
    private int _currentModeIndex;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(entityEditor);
        Assert.IsNotNull(cam);
        
        _map = new MapSpace(initMapSize);
        _signalCircuit = new SignalCircuit();

        entityEditor.Inject(cam);
        entityEditor.Inject(_map);
        entityEditor.Inject(_signalCircuit);
        
        _editorModes = new IMapEditorMode[]
        {
            entityEditor
        };
        _currentModeIndex = 0;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string Pack()
    {
        var mapData = new MapData()
        {
            SpaceSize = _map.MapSize,
            LayerNames = new string[_map.EntitiesCount],
            LayerData = new string[_map.EntitiesCount],
            SignalData = _signalCircuit.Pack()
        };
        
        for (var i = 0; i < _map.EntitiesCount; i++)
        {
            var entity = _map.GetEntity(i);
            mapData.LayerNames[i] = entity.Title;
            mapData.LayerData[i] = entity.Pack();
        }
        
        return mapData.Pack();
    }

    public void Unpack(string data)
    {
        _editorModes[_currentModeIndex].Exit();
        
        while (_map.EntitiesCount > 0)
        {
            var dead = _map.GetEntity(0);
            _map.UnregisterAt(0);
            dead.Clear();
        }
        _map.Kill();
        
        var chapterData = new MapData();
        chapterData.Unpack(data);
        
        _map = new MapSpace(chapterData.SpaceSize);
        for (var i = 0; i < chapterData.LayerNames.Length; i++)
        {
            var entity = GlobalConfig.Ins.entityFactory.CreateEntity(chapterData.LayerNames[i]);
            _map.RegisterAt(entity, i);
            entity.Unpack(chapterData.LayerData[i]);
        }
        
        _signalCircuit.Unpack(chapterData.SignalData);
        
        entityEditor.Inject(_map);
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
        cam.orthographicSize = Mathf.Max(cam.orthographicSize, cameraMinSize);

        cam.orthographicSize = Mathf.Min(cam.orthographicSize, worldSize.y * 0.5f / (1f - cameraRelativeClearance));
        cam.orthographicSize = Mathf.Min(cam.orthographicSize, worldSize.x * 0.5f / (cam.aspect - cameraRelativeClearance));

        var newMouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        var worldOffset = prevMouseWorldPos - newMouseWorldPos;
        cam.transform.position += worldOffset;

        var halfHeight = cam.orthographicSize;
        var halfWidth = halfHeight * cam.aspect;

        var worldStart = _map.WorldStart;
        var worldEnd = worldStart + worldSize;

        var clearance = halfHeight * cameraRelativeClearance;
        var x = cam.transform.position.x + horizontal * halfHeight * cameraMoveSpeed * Time.deltaTime;
        x = Mathf.Max(x - halfWidth, worldStart.x - clearance) + halfWidth;
        x = Mathf.Min(x + halfWidth, worldEnd.x + clearance) - halfWidth;

        var y = cam.transform.position.y + vertical * halfHeight * cameraMoveSpeed * Time.deltaTime;
        y = Mathf.Max(y - halfHeight, worldStart.y - clearance) + halfHeight;
        y = Mathf.Min(y + halfHeight, worldEnd.y + clearance) - halfHeight;

        cam.transform.position = new Vector3(x, y, cam.transform.position.z);


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