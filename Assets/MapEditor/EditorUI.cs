using System;
using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace MapEditor
{
public class EditorUI : MonoBehaviour 
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private EditorController editorController;
    [SerializeField] private UIDocument document;
    [SerializeField] private EventSystem eventSystem;

    private VisualElement _root;

    private VisualElement _entityPropertiesContainer;
    private DropdownField _entityCreationDropdown;
    private VisualElement _savingIsle;
    private TextField _pathInputField;
    private Button _saveButton;
    private Button _loadButton;
    private Button _deleteButton;
    
    private MapEntity _mapEntity;
    
    private bool _mouseOverUI;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake() 
    {
        Assert.IsNotNull(editorController);
        Assert.IsNotNull(document);
        Assert.IsNotNull(eventSystem);
        
        eventSystem.sendNavigationEvents = false;
        
        _root = document.rootVisualElement;
        
        _entityPropertiesContainer = _root.Q<VisualElement>("entityPropertiesContainer");
        _entityCreationDropdown = _root.Q<DropdownField>("entityCreationDropdown");
        _entityCreationDropdown.choices = new List<string>(GlobalConfig.Ins.entityFactory.GetEntityTitles());
        _entityCreationDropdown.index = 0;

        _savingIsle = _root.Q<VisualElement>("savingIsle");
        _pathInputField = _root.Q<TextField>("pathInputField");
        _saveButton = _root.Q<Button>("saveButton");
        _loadButton = _root.Q<Button>("loadButton");
        _deleteButton = _root.Q<Button>("deleteButton");
        
        _saveButton.clicked += OnSaveButtonClicked;
        _loadButton.clicked += OnLoadButtonClicked;
        _deleteButton.clicked += OnDeleteButtonClicked;

        _savingIsle.RegisterCallback<MouseEnterEvent>(_ => _mouseOverUI = true);
        _savingIsle.RegisterCallback<MouseLeaveEvent>(_ => _mouseOverUI = false);
        _entityCreationDropdown.RegisterCallback<MouseEnterEvent>(_ => _mouseOverUI = true);
        _entityCreationDropdown.RegisterCallback<MouseLeaveEvent>(_ => _mouseOverUI = false);
        _entityPropertiesContainer.RegisterCallback<MouseEnterEvent>(_ => _mouseOverUI = true);
        _entityPropertiesContainer.RegisterCallback<MouseLeaveEvent>(_ => _mouseOverUI = false);
        
        UpdateFields();
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetEnabled(bool value)
    {
        _root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        _root.SetEnabled(value);
        if (value)
            UpdateFields();
    }
    
    public void SetEntity(MapEntity entity)
    {
        _mapEntity = entity;
        _mapEntity.PropertiesChangeEvent += UpdateFields;
        UpdateFields();
    }

    public void ClearEntity()
    {
        _mapEntity.PropertiesChangeEvent -= UpdateFields;
        _mapEntity = null;
        UpdateFields();
    }
    
    public string GetSelectedEntityTitleForCreation() => _entityCreationDropdown.value;
    
    
    //game loop//////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update() => UpdateInputConsumption();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void UpdateInputConsumption()
    {
        EditorController.CanEdit = !_mouseOverUI && !_entityCreationDropdown.;
    }
    
    private void UpdateFields()
    {
        _entityPropertiesContainer.Clear();

        if (!_mapEntity)
        {
            var noLayerTextContainer = new VisualElement();
            noLayerTextContainer.AddToClassList("flex-row");
            noLayerTextContainer.AddToClassList("items-center");
            noLayerTextContainer.AddToClassList("mb-2");
            
            var noLayerDescLabel = new Label("No entity selected");
            noLayerDescLabel.AddToClassList("text-gray-700");
            noLayerTextContainer.Add(noLayerDescLabel);
            
            _entityPropertiesContainer.Add(noLayerTextContainer);
            return;
        }

        // Entity layer info
        var layerTextContainer = new VisualElement();
        layerTextContainer.AddToClassList("flex-row");
        layerTextContainer.AddToClassList("items-center");
        layerTextContainer.AddToClassList("mb-2");
        
        var layerDescLabel = new Label("Entity layer:");
        layerDescLabel.AddToClassList("");

        var indexLabel = new Label(_mapEntity.Layer.ToString());
        indexLabel.AddToClassList("font-medium");

        layerTextContainer.Add(layerDescLabel);
        layerTextContainer.Add(indexLabel);
        _entityPropertiesContainer.Add(layerTextContainer);

        // Property fields
        var handles = _mapEntity.GetProperties();
        while (handles.MoveNext())
        {
            var handle = handles.Current;

            var propertyContainer = new VisualElement();
            

            var propertyNameLabel = new Label(handle.PropertyName);
            propertyNameLabel.AddToClassList("min-w-24");
            propertyContainer.Add(propertyNameLabel);

            switch (handle.PropertyType)
            {
                case PropertyType.Decimal:
                    var floatValueField = new FloatField { value = (float)handle.Getter.Invoke() };
                    
                    floatValueField.AddToClassList("min-w-40");
                    floatValueField.AddToClassList("bg-white");
                    floatValueField.AddToClassList("border");
                    floatValueField.AddToClassList("border-gray-300");
                    
                    floatValueField.RegisterCallback<FocusOutEvent>(e =>
                    {
                        handle.Setter?.Invoke(floatValueField.value);
                        floatValueField.value = (float)handle.Getter.Invoke();
                    });
                    
                    propertyContainer.Add(floatValueField);
                    break;
                
                case PropertyType.Integer:
                    var intValueField = new IntegerField { value = (int)handle.Getter.Invoke() };
                    
                    intValueField.AddToClassList("min-w-40");
                    intValueField.AddToClassList("bg-white");
                    intValueField.AddToClassList("border");
                    intValueField.AddToClassList("border-gray-300");
                    
                    intValueField.RegisterCallback<FocusOutEvent>(e =>
                    {
                        handle.Setter?.Invoke(intValueField.value);
                        intValueField.value = (int)handle.Getter.Invoke();
                    });
                    propertyContainer.Add(intValueField);
                    break;
                
                case PropertyType.Text:
                    var textValueField = new TextField { value = handle.Getter.Invoke().ToString() };
                    
                    textValueField.AddToClassList("min-w-40");
                    textValueField.AddToClassList("bg-white");
                    textValueField.AddToClassList("border");
                    textValueField.AddToClassList("border-gray-300");
                    
                    textValueField.RegisterCallback<FocusOutEvent>(e =>
                    {
                        handle.Setter?.Invoke(textValueField.value);
                        textValueField.value = handle.Getter.Invoke().ToString();
                        Debug.Log("Unfocus");
                    });
                    
                    propertyContainer.Add(textValueField);
                    break;
                
                case PropertyType.Bool:
                    var boolValueField = new Toggle { value = (bool)handle.Getter.Invoke() };
                    
                    boolValueField.AddToClassList("bg-white");
                    
                    boolValueField.RegisterCallback<ChangeEvent<bool>>(e =>
                    {
                        handle.Setter?.Invoke(boolValueField.value);
                        boolValueField.value = (bool)handle.Getter.Invoke();
                    });
                    
                    propertyContainer.Add(boolValueField);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _entityPropertiesContainer.Add(propertyContainer);


            propertyContainer.AddToClassList("flex-row");
            propertyContainer.AddToClassList("items-center");
            propertyContainer.AddToClassList("mb-2");
        }
        
        _entityPropertiesContainer.Blur();
    }

    private void OnSaveButtonClicked()
    {
        var fileName = _pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        var content = editorController.Pack();
        MapSaveManager.SaveAs(fileName, content);
    }

    private void OnLoadButtonClicked()
    {
        var fileName = _pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        if (MapSaveManager.Load(fileName, out var content))
            editorController.Unpack(content);
    }

    private void OnDeleteButtonClicked()
    {
        var fileName = _pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        MapSaveManager.Delete(fileName);
    }
}
}