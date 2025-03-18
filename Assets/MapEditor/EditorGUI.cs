using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using TMPro;
using Map;

namespace MapEditor
{

public class EditorGUI : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private TMP_InputField pathInputField;
    [SerializeField] private ButtonWithEvents saveButton;
    [SerializeField] private ButtonWithEvents loadButton;
    [SerializeField] private ButtonWithEvents deleteButton;
    [SerializeField] private TMP_Dropdown entityCreationDropdown;
    [Space]
    [SerializeField] private RectTransform entityPropertiesPanel;
    [SerializeField] private TMP_Text currentLayerText;
    [SerializeField] private GameObject propertyFieldPrefab;
    [Space]
    [SerializeField] private TMP_Dropdown dropdown;
    [Space]
    [SerializeField] private EditorController editorController;
    
    private readonly List<TextPropertyUIField> _uiFields = new();
    private IPropertyHolder _propertyHolder = null;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(pathInputField);
        Assert.IsNotNull(saveButton);
        Assert.IsNotNull(loadButton);
        Assert.IsNotNull(deleteButton);
        
        Assert.IsNotNull(entityPropertiesPanel);
        Assert.IsNotNull(currentLayerText);
        Assert.IsNotNull(propertyFieldPrefab);
        
        Assert.IsNotNull(dropdown);
        
        Assert.IsNotNull(editorController);

        
        saveButton.onClick.AddListener(HandleSaveButtonClick);
        loadButton.onClick.AddListener(HandleLoadButtonClick);
        deleteButton.onClick.AddListener(HandleDeleteButtonClick);
        
        currentLayerText.text = "No Layer selected";
        
        var entityTitles = GlobalConfig.Ins.entityFactory.GetEntityTitles();
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(entityTitles));
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveListener(HandleSaveButtonClick);
        loadButton.onClick.RemoveListener(HandleLoadButtonClick);
        deleteButton.onClick.RemoveListener(HandleDeleteButtonClick);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public string GetSelectedTitleForCreation() => dropdown.options[dropdown.value].text;
    
    public void SetEnabled(bool value) => entityPropertiesPanel.gameObject.SetActive(value);
    
    public void SetEntity(MapEntity entity)
    {
        currentLayerText.text = "Entity layer: " + entity.Layer;
        _propertyHolder = entity;
        _propertyHolder.PropertiesChangeEvent += UpdatePropertyFields;
        
        UpdatePropertyFields();
    }

    public void ClearEntity()
    {
        _propertyHolder.PropertiesChangeEvent -= UpdatePropertyFields;
        _propertyHolder = null;
        currentLayerText.text = "No layer selected";
        
        UpdatePropertyFields();
    }
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        EditorController.CanEdit = !EventSystem.current.IsPointerOverGameObject() && !pathInputField.isFocused && !entityCreationDropdown.IsExpanded;
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdatePropertyFields()
    {
        foreach (var field in _uiFields) 
            Destroy(field.Target.gameObject);

        EditorController.CanEdit = true;
        _uiFields.Clear();

        currentLayerText.rectTransform.anchoredPosition = new Vector2(0, 0);

        
        if (_propertyHolder == null)
            return;
        
        var handles = _propertyHolder.GetProperties();
        while (handles.MoveNext())
        {
            var field = Instantiate(propertyFieldPrefab, entityPropertiesPanel).GetComponent<TextPropertyUIField>();
            Assert.IsNotNull(field);

            field.SetProperty(handles.Current);

            _uiFields.Add(field);
        }

        float yOffset = 0;
        for (var i = _uiFields.Count - 1; i >= 0; i--)
        {
            var rect = _uiFields[i].Target;
            rect.anchoredPosition = new Vector2(0, yOffset);
            yOffset += rect.sizeDelta.y;
        }

        currentLayerText.rectTransform.anchoredPosition = new Vector2(0, yOffset);
    }
    
    private void HandleSaveButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        var content = editorController.Pack();
        MapSaveManager.SaveAs(fileName, content);
    }

    private void HandleLoadButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        if (MapSaveManager.Load(fileName, out var content))
            editorController.Unpack(content);
    }

    private void HandleDeleteButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        MapSaveManager.Delete(fileName);
    }
}

}