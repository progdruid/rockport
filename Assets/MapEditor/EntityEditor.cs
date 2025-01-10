using UnityEngine;
using UnityEngine.Assertions;
using Map;

namespace MapEditor
{

public class EntityEditor : MonoBehaviour, IMapEditorMode
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private EntityUIPanel entityUIPanel;
    [Space] 
    [SerializeField] private string alpha1LayerTitle;
    [SerializeField] private string alpha2LayerTitle;
    [SerializeField] private string alpha3LayerTitle;
    [SerializeField] private string alpha4LayerTitle;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;

    private int _selectedLayer = -1;
    private MapEntity _selectedEntity;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(entityUIPanel);

        Assert.IsNotNull(alpha1LayerTitle);
        Assert.IsNotNull(alpha2LayerTitle);
        Assert.IsNotNull(alpha3LayerTitle);
        Assert.IsNotNull(alpha4LayerTitle);
        Assert.IsFalse(alpha1LayerTitle.Length == 0);
        Assert.IsFalse(alpha2LayerTitle.Length == 0);
        Assert.IsFalse(alpha3LayerTitle.Length == 0);
        Assert.IsFalse(alpha4LayerTitle.Length == 0);
    }
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void Inject(MapSpace map) => _map = map;
    public void Inject(SignalCircuit circuit) => _signalCircuit = circuit;

    public void Enter()
    {
        var previouslySelectedLayer = _selectedLayer;
        UnselectLayer();
        SelectLayer(previouslySelectedLayer);
        
        entityUIPanel.SetEnabled(true);
    }

    public void Exit()
    {
        entityUIPanel.SetEnabled(false);
    }

    public void HandleInput(Vector2 worldMousePos)
    {
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
                if (_map.GetEntity(i).CheckOverlap(worldMousePos))
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
            _selectedEntity.ChangeAt(worldMousePos, constructive);
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateLayer(string layerTitle)
    {
        var layer = _selectedLayer + 1;
        UnselectLayer();
        
        var entity = GlobalConfig.Ins.entityFactory.CreateEntity(layerTitle);
        _map.RegisterAt(entity, layer);
        _signalCircuit.ExtractAndAdd(entity);
        
        SelectLayer(layer);
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
        entityUIPanel.UpdateWithEntity(_selectedEntity);
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !_map.HasLayer(layer))
            return;

        _selectedLayer = layer;
        _selectedEntity = _map.GetEntity(layer);
        entityUIPanel.UpdateWithEntity(_selectedEntity);
    }

    private void UnselectLayer()
    {
        if (_selectedLayer == -1)
            return;

        entityUIPanel.ClearEntity();
        _selectedEntity = null;
        _selectedLayer = -1;
    }
}
}