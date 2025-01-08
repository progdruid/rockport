
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using Map;

namespace MapEditor
{

/// <summary>
/// sdcjshdcjhsdbcjshdbc <see cref="MonoBehaviour"/>
/// </summary>
public class EditorController : MonoBehaviour, IPackable
{
    /// TODO: should be extracted to a separate UI system
    public static bool s_CanEdit = true;

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private EntityFactory entityFactory;
    [SerializeField] private EntityUIPanel entityUIPanel;
    [SerializeField] private TMP_Text layerText;
    [Space] 
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float cameraMinSize;
    [SerializeField] private float cameraRelativeClearance;
    [Space] 
    [SerializeField] private Vector2Int initMapSize;
    [Space] 
    [SerializeField] private string alpha1LayerTitle;
    [SerializeField] private string alpha2LayerTitle;
    [SerializeField] private string alpha3LayerTitle;
    [SerializeField] private string alpha4LayerTitle;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;
    
    private int _selectedLayer = -1;
    private MapEntity _selectedEntity = null;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(entityFactory);
        Assert.IsNotNull(entityUIPanel);
        Assert.IsNotNull(layerText);

        Assert.IsNotNull(cam);

        Assert.IsNotNull(alpha1LayerTitle);
        Assert.IsNotNull(alpha2LayerTitle);
        Assert.IsNotNull(alpha3LayerTitle);
        Assert.IsNotNull(alpha4LayerTitle);
        Assert.IsFalse(alpha1LayerTitle.Length == 0);
        Assert.IsFalse(alpha2LayerTitle.Length == 0);
        Assert.IsFalse(alpha3LayerTitle.Length == 0);
        Assert.IsFalse(alpha4LayerTitle.Length == 0);
        
        _map = new MapSpace(initMapSize);
        _signalCircuit = new SignalCircuit();
        
        UpdateLayerText();
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
        UnselectLayer();
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
            var entity = entityFactory.CreateEntity(chapterData.LayerNames[i]);
            _map.RegisterAt(entity, i);
            entity.Unpack(chapterData.LayerData[i]);
        }
        
        _signalCircuit.Unpack(chapterData.SignalData);
    }

    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!s_CanEdit) return;
        
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

        
        // mouse pos in world
        var mousePos = Input.mousePosition;
        var ray = cam.ScreenPointToRay(mousePos);

        var t = - ray.origin.z / ray.direction.z;
        var worldPos = (ray.origin + ray.direction * t);
        
        
        //layer creation
        if (Input.GetKeyDown(KeyCode.Alpha1))
            CreateLayer(alpha1LayerTitle);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CreateLayer(alpha2LayerTitle);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CreateLayer(alpha3LayerTitle);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            CreateLayer(alpha4LayerTitle);
        
        //layer deletion
        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteLayer();

        // spawn point management
        if (Input.GetKeyDown(KeyCode.P))
        {
            var layer = _map.FindEntity(GlobalConfig.Ins.spawnPointEntityName, out _);
            if (layer < 0) CreateLayer(GlobalConfig.Ins.spawnPointEntityName);
            else SelectLayer(layer);
        }
        
        //moving and changing layers
        var selectDirection = (Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0) +
                              (Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0);
        if (selectDirection != 0 && Input.GetKey(KeyCode.LeftControl))
            MoveLayer(selectDirection);
        else if (selectDirection != 0 && _selectedLayer + selectDirection >= 0)
            SelectLayer(_map.ClampLayer(_selectedLayer + selectDirection));
        else if (selectDirection != 0 && _selectedLayer + selectDirection < 0)
            UnselectLayer();

        //mouse select
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            UnselectLayer();
            for (var i = _map.EntitiesCount - 1; i >= 0; i--)
                if (_map.GetEntity(i).CheckOverlap(worldPos))
                {
                    SelectLayer(i);
                    break;
                }
        }
        
        if (!_selectedEntity || Input.GetKey(KeyCode.LeftShift))
            return;
        
        // place/remove
        var constructive = Input.GetMouseButton(0);
        var destructive = Input.GetMouseButton(1);
            
        if (constructive != destructive)
            _selectedEntity.ChangeAt(worldPos, constructive);
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateLayer(string layerTitle)
    {
        var layer = _selectedLayer + 1;
        UnselectLayer();
        
        var entity = entityFactory.CreateEntity(layerTitle);
        _map.RegisterAt(entity, layer);
        _signalCircuit.ExtractAndAdd(entity);
        
        SelectLayer(layer);

        //updating camera position, so it is always behind the topmost layer
        var z = _map.GetTopmostEntity().GetReferenceZ() - 1;
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, z);
    }

    private void DeleteLayer()
    {
        var layer = _selectedLayer;
        if (!_map.HasLayer(layer))
            return;

        UnselectLayer();
        
        var dead = _map.GetEntity(layer);
        _signalCircuit.ExtractAndRemove(dead);
        _map.UnregisterAt(layer);
        dead.Clear();

        SelectLayer(_map.ClampLayer(layer));
    }


    private void MoveLayer(int dir)
    {
        var layerTo = _selectedLayer + dir;
        if (!_map.MoveLayer(_selectedLayer, layerTo))
            return;
        _selectedLayer = layerTo;
        UpdateLayerText();
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !_map.HasLayer(layer))
            return;

        _selectedLayer = layer;
        _selectedEntity = _map.GetEntity(layer);
        entityUIPanel.SetPropertyHolder(_selectedEntity);
        UpdateLayerText();
    }

    private void UnselectLayer()
    {
        if (_selectedLayer == -1)
            return;

        entityUIPanel.UnsetPropertyHolder();
        _selectedEntity = null;
        _selectedLayer = -1;
        UpdateLayerText();
    }

    private void UpdateLayerText() =>
        layerText.text = $"Selected Layer: {_selectedLayer} " +
                         (_selectedEntity ? "Entity layer: " + _selectedEntity.Layer : "No layer selected");
}

}