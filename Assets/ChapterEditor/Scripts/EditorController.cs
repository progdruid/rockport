
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class EditorController : MonoBehaviour, IPackable
{
    /// TODO: should be extracted to a separate UI system
    public static bool s_CanEdit = true;

    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private LayerFactory layerFactory;
    [SerializeField] private ManipulatorUIPanel manipulatorUIPanel;
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
    
    private MapSpaceHolder _holder;
    
    private int _selectedLayer = -1;
    private ManipulatorBase _selectedManipulator = null;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(layerFactory);
        Assert.IsNotNull(manipulatorUIPanel);
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

        _holder = new MapSpaceHolder(initMapSize);
        
        UpdateLayerText();
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////

    public string Pack()
    {
        var chapterData = new MapData()
        {
            SpaceSize = _holder.MapSize,
            LayerNames = new string[_holder.ManipulatorsCount],
            LayerData = new string[_holder.ManipulatorsCount]
        };
        
        for (var i = 0; i < _holder.ManipulatorsCount; i++)
        {
            var manipulator = _holder.GetManipulator(i);
            chapterData.LayerNames[i] = manipulator.ManipulatorName;
            chapterData.LayerData[i] = manipulator.Pack();
        }
        
        return chapterData.Pack();
    }

    public void Unpack(string data)
    {
        var chapterData = new MapData();
        chapterData.Unpack(data);

        UnselectLayer();
        while (_holder.ManipulatorsCount > 0)
        {
            var dead = _holder.GetManipulator(0);
            _holder.UnregisterAt(0);
            dead.Clear();
        }
        
        _holder.Kill();
        _holder = new MapSpaceHolder(chapterData.SpaceSize);

        for (var i = 0; i < chapterData.LayerNames.Length; i++)
        {
            var manipulator = layerFactory.CreateManipulator(chapterData.LayerNames[i]);
            _holder.RegisterAt(manipulator, i);
            manipulator.Unpack(chapterData.LayerData[i]);
        }
    }

    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!s_CanEdit) return;
        
        // camera
        var horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        var vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        var zoom = -Input.mouseScrollDelta.y;

        var worldSize = _holder.WorldSize;
        
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

        var worldStart = _holder.WorldStart;
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
            var layer = _holder.FindManipulator(GlobalConfig.Ins.spawnPointManipulatorName, out _);
            if (layer < 0) CreateLayer(GlobalConfig.Ins.spawnPointManipulatorName);
            else SelectLayer(layer);
        }
        
        //moving and changing layers
        var selectDirection = (Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0) +
                              (Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0);
        if (selectDirection != 0 && Input.GetKey(KeyCode.LeftControl))
            MoveLayer(selectDirection);
        else if (selectDirection != 0 && _selectedLayer + selectDirection >= 0)
            SelectLayer(_holder.ClampLayer(_selectedLayer + selectDirection));
        else if (selectDirection != 0 && _selectedLayer + selectDirection < 0)
            UnselectLayer();

        //mouse select
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            UnselectLayer();
            for (var i = _holder.ManipulatorsCount - 1; i >= 0; i--)
                if (_holder.GetManipulator(i).CheckOverlap(worldPos))
                {
                    SelectLayer(i);
                    break;
                }
        }
        
        if (!_selectedManipulator || Input.GetKey(KeyCode.LeftShift))
            return;
        
        // place/remove
        var constructive = Input.GetMouseButton(0);
        var destructive = Input.GetMouseButton(1);
            
        if (constructive != destructive)
            _selectedManipulator.ChangeAt(worldPos, constructive);
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateLayer(string layerTitle)
    {
        var layer = _selectedLayer + 1;
        UnselectLayer();
        _holder.RegisterAt(layerFactory.CreateManipulator(layerTitle), layer);
        SelectLayer(layer);

        //updating camera position, so it is always behind the topmost layer
        var z = _holder.GetTopmostManipulator().GetReferenceZ() - 1;
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, z);
    }

    private void DeleteLayer()
    {
        var layer = _selectedLayer;
        if (!_holder.HasLayer(layer))
            return;

        UnselectLayer();
        var dead = _holder.GetManipulator(layer);
        _holder.UnregisterAt(layer);
        dead.Clear();

        SelectLayer(_holder.ClampLayer(layer));
    }


    private void MoveLayer(int dir)
    {
        var layerTo = _selectedLayer + dir;
        if (!_holder.MoveLayer(_selectedLayer, layerTo))
            return;
        _selectedLayer = layerTo;
        UpdateLayerText();
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !_holder.HasLayer(layer))
            return;

        _selectedLayer = layer;
        UpdateLayerText();
        _selectedManipulator = _holder.GetManipulator(layer);
        manipulatorUIPanel.SetPropertyHolder(_selectedManipulator);
    }

    private void UnselectLayer()
    {
        if (_selectedLayer == -1)
            return;

        manipulatorUIPanel.UnsetPropertyHolder();
        _selectedManipulator = null;
        _selectedLayer = -1;
        UpdateLayerText();
    }

    private void UpdateLayerText() =>
        layerText.text = _selectedLayer != -1 ? "Layer: " + _selectedLayer : "No layer selected"; 
}

}