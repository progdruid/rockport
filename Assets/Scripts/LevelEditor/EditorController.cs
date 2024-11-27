using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class EditorController : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private LevelSpaceHolder holder;
    [SerializeField] private ManipulatorUIPanel manipulatorUIPanel;
    [SerializeField] private TMP_Text layerText;
    [Space] 
    [SerializeField] private GameObject dirtPrefab;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject treeBackgroundPrefab;
    [SerializeField] private GameObject objectPrefab;
    [Space]
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float cameraMinSize;
    
    private int _selectedLayer = -1;
    private IPlaceRemoveHandler _placeRemoveHandler = null;

    private bool _canEdit = true;

    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(holder);
        Assert.IsNotNull(manipulatorUIPanel);
        Assert.IsNotNull(layerText);
        
        Assert.IsNotNull(dirtPrefab);
        Assert.IsNotNull(treePrefab);
        Assert.IsNotNull(treeBackgroundPrefab);
        Assert.IsNotNull(objectPrefab);
        
        Assert.IsNotNull(cam);

        UpdateLayerText();
        manipulatorUIPanel.ConsumeInputChangeEvent += consumesInput => _canEdit = !consumesInput;
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetPlaceRemoveHandler(IPlaceRemoveHandler handler) => _placeRemoveHandler = handler;
    public void UnsetPlaceRemoveHandler() => _placeRemoveHandler = null;
    
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (!_canEdit) return;

        CheckLayerLogic();
        CheckCameraMovement();
        
        if (_placeRemoveHandler != null)
            CheckPlaceRemove();
    }

    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void CheckLayerLogic()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            CreateLayer(dirtPrefab);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CreateLayer(treePrefab);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CreateLayer(treeBackgroundPrefab);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            CreateLayer(objectPrefab);

        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteLayer();
        
        var selectDirection = (Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0) +
                              (Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0);
        if (selectDirection != 0 && Input.GetKey(KeyCode.LeftControl))
            MoveLayer(selectDirection);
        else if (selectDirection != 0)
            SelectLayer(holder.ClampLayer(_selectedLayer + selectDirection));
    }

    private void CreateLayer(GameObject prefab)
    {
        var layer = _selectedLayer + 1;
        UnselectLayer();
        
        var go = Instantiate(prefab);
        var manipulator = go.GetComponent<ManipulatorBase>();
        holder.RegisterAt(manipulator, layer);
        
        SelectLayer(layer);
    }

    private void DeleteLayer()
    {
        var layer = _selectedLayer;
        if (!holder.HasLayer(layer))
            return;
        
        UnselectLayer();
        var dead = holder.GetManipulator(layer);
        holder.UnregisterAt(layer);
        Destroy(dead.Target.gameObject);
        
        SelectLayer(holder.ClampLayer(layer));
    }
    
    
    private void MoveLayer(int dir)
    {
        var layerTo = _selectedLayer + dir;
        if (!holder.MoveRegister(_selectedLayer, layerTo)) 
            return;
        _selectedLayer = layerTo;
        UpdateLayerText();
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !holder.HasLayer(layer))
            return;
        
        _selectedLayer = layer;
        UpdateLayerText();
        var manipulator = holder.GetManipulator(layer);
        manipulator.SubscribeInput(this);
        manipulatorUIPanel.SetPropertyHolder(manipulator);
    }

    private void UnselectLayer()
    {
        if (_selectedLayer == -1)
            return;
        
        var manipulator = holder.GetManipulator(_selectedLayer);
        manipulator.UnsubscribeInput();
        manipulatorUIPanel.UnsetPropertyHolder();
        _selectedLayer = -1;
        UpdateLayerText();
    }

    private void UpdateLayerText() =>
        layerText.text = _selectedLayer != -1 ? "Layer: " + _selectedLayer : "No layer selected";

    private void CheckPlaceRemove()
    {
        var constructive = Input.GetMouseButton(0);
        var destructive = Input.GetMouseButton(1);

        if (constructive == destructive)
            return;

        var mousePos = Input.mousePosition;
        var ray = cam.ScreenPointToRay(mousePos);

        var t = (_placeRemoveHandler.GetZForInteraction() - ray.origin.z) / ray.direction.z;
        var worldPos = (ray.origin + ray.direction * t);

        _placeRemoveHandler.ChangeAt(worldPos, constructive && !destructive);
    }

    private void CheckCameraMovement()
    {
        var horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        var vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        var zoom = (Input.GetKey(KeyCode.E) ? -1 : 0) + (Input.GetKey(KeyCode.Q) ? 1 : 0);

        cam.orthographicSize += zoom * cameraZoomSpeed * cam.orthographicSize * Time.deltaTime;
        cam.orthographicSize = Mathf.Max(cam.orthographicSize, cameraMinSize);
        
        var worldSize = holder.WorldSize;
        cam.orthographicSize = Mathf.Min(cam.orthographicSize, worldSize.y * 0.5f);
        cam.orthographicSize = Mathf.Min(cam.orthographicSize*cam.aspect, worldSize.x * 0.5f) / cam.aspect;
        
        var halfHeight = cam.orthographicSize;
        var halfWidth = halfHeight * cam.aspect;

        var worldStart = holder.WorldStart;
        var worldEnd = worldStart + worldSize;

        var x = cam.transform.position.x + horizontal * halfHeight * cameraMoveSpeed * Time.deltaTime;
        x = Mathf.Max(x - halfWidth, worldStart.x) + halfWidth;
        x = Mathf.Min(x + halfWidth, worldEnd.x) - halfWidth;
        
        var y = cam.transform.position.y + vertical * halfHeight * cameraMoveSpeed * Time.deltaTime;
        y = Mathf.Max(y - halfHeight, worldStart.y) + halfHeight;
        y = Mathf.Min(y + halfHeight, worldEnd.y) - halfHeight;
        
        cam.transform.position = new Vector3(x, y, cam.transform.position.z);
    }
}