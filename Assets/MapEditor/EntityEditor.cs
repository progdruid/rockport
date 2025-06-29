﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Map;
using TMPro;

namespace MapEditor
{

public class EntityEditor : MonoBehaviour, IMapEditorMode
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private EditorGUI editorGUI;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;
    
    private int _selectedLayer = -1;
    private MapEntity _selectedEntity;
    private Action<Vector2, bool> _selectedEntityAction = null;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(editorGUI);
    }
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void Inject(MapSpace map) => _map = map;
    public void Inject(SignalCircuit circuit) => _signalCircuit = circuit;

    public void Enter()
    {
        var previouslySelectedLayer = _selectedLayer;
        UnselectLayer();
        SelectLayer(previouslySelectedLayer);
        
        editorGUI.SetEnabled(true);
    }

    public void Exit()
    {
        editorGUI.SetEnabled(false);
    }

    public void HandleInput(Vector2 worldMousePos)
    {
        //layer creation
        if (Input.GetKeyDown(KeyCode.N))
        {
            var layerTitle = editorGUI.GetSelectedTitleForCreation();
            CreateLayer(layerTitle);
        }
        
        //layer deletion
        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteLayer();

        //unselect
        if (Input.GetKeyDown(KeyCode.Escape))
            UnselectLayer();
        
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
                if (_map.GetEntity(i).Accessors.TryGetValue("overlap", out var accessor)
                    && accessor is EntityOverlapAccessor overlapAccessor
                    && overlapAccessor.CheckOverlap(worldMousePos))
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
            _selectedEntityAction.Invoke(worldMousePos, constructive);
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateLayer(string layerTitle)
    {
        var layer = _selectedLayer + 1;
        UnselectLayer();
        
        var entity = GlobalConfig.Ins.entityFactory.CreateEntity(layerTitle);
        _map.RegisterAt(entity, layer);
        entity.Initialise();
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
        editorGUI.SetEntity(_selectedEntity);
    }

    private void SelectLayer(int layer)
    {
        if (layer == _selectedLayer || !_map.HasLayer(layer))
            return;

        _selectedLayer = layer;
        _selectedEntity = _map.GetEntity(layer);
        editorGUI.SetEntity(_selectedEntity);
        
        if (_selectedEntity.Accessors.TryGetValue("anchor", out var anchorRawAccessor))
        {
            Assert.IsTrue(anchorRawAccessor is IAnchorAccessor);
            var anchorAccessor = (IAnchorAccessor)anchorRawAccessor;
            _selectedEntityAction = (worldPos, constructive) => anchorAccessor.SetPosition(worldPos);
        }
        else if (_selectedEntity.Accessors.TryGetValue("tile-layer", out var tileRawAccessor))
        {
            Assert.IsTrue(tileRawAccessor is ITileLayerAccessor);
            var tileAccessor = (ITileLayerAccessor)tileRawAccessor;
            _selectedEntityAction = (worldPos, constructive) => tileAccessor.ChangeAtWorldPos(worldPos, constructive);
        }
    }

    private void UnselectLayer()
    {
        if (_selectedLayer == -1)
            return;

        editorGUI.ClearEntity();
        _selectedEntity = null;
        _selectedLayer = -1;
        _selectedEntityAction = null;
    }
}
}