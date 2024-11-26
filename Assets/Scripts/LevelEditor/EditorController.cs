using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class EditorController : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private LevelSpaceHolder holder;
    [SerializeField] private Camera controlCamera;
    [SerializeField] private ManipulatorUIPanel manipulatorUIPanel;
    [Space] 
    [SerializeField] private GameObject treeBackgroundPrefab;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject dirtPrefab;
    [SerializeField] private GameObject objectPrefab;

    private int _selectedLayer = -1;
    private IPlaceRemoveHandler _placeRemoveHandler = null;

    private bool _canEdit = true;

    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(holder);
        Assert.IsNotNull(controlCamera);
        Assert.IsNotNull(manipulatorUIPanel);
        Assert.IsNotNull(treeBackgroundPrefab);
        Assert.IsNotNull(treePrefab);
        Assert.IsNotNull(dirtPrefab);
        Assert.IsNotNull(objectPrefab);

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
        
        var selectDirection = (Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0) +
                              (Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0);
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
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !holder.HasLayer(layer))
            return;
        
        _selectedLayer = layer;
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
    }
    
    private void CheckPlaceRemove()
    {
        var constructive = Input.GetMouseButton(0);
        var destructive = Input.GetMouseButton(1);

        if (constructive == destructive)
            return;

        var mousePos = Input.mousePosition;
        var ray = controlCamera.ScreenPointToRay(mousePos);

        var t = (_placeRemoveHandler.GetZForInteraction() - ray.origin.z) / ray.direction.z;
        var worldPos = (ray.origin + ray.direction * t);

        _placeRemoveHandler.ChangeAt(worldPos, constructive && !destructive);
    }
}