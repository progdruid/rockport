using System.Collections.Generic;
using Map;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace MapEditor
{

public class EntityUIPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private RectTransform targetPanel;
    [SerializeField] private TMP_Text layerText;
    [SerializeField] private GameObject propertyFieldPrefab;

    private readonly List<TextPropertyUIField> _uiFields = new();
    private IPropertyHolder _propertyHolder = null;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(targetPanel);
        Assert.IsNotNull(layerText);
        Assert.IsNotNull(propertyFieldPrefab);
        
        layerText.text = "No Layer selected";
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetEnabled(bool value) => targetPanel.gameObject.SetActive(value);
    
    public void UpdateWithEntity(MapEntity entity)
    {
        layerText.text = "Entity layer: " + entity.Layer;
        _propertyHolder = entity;
        _propertyHolder.PropertiesChangeEvent += CreateUIFields;
        CreateUIFields();
    }

    public void ClearEntity()
    {
        ClearProperties();
        _propertyHolder.PropertiesChangeEvent -= CreateUIFields;
        _propertyHolder = null;
        layerText.text = "No layer selected";
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateUIFields()
    {
        ClearProperties();

        Assert.IsNotNull(_propertyHolder);
        var handles = _propertyHolder.GetProperties();
        while (handles.MoveNext())
        {
            var field = Instantiate(propertyFieldPrefab, targetPanel).GetComponent<TextPropertyUIField>();
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

        layerText.rectTransform.anchoredPosition = new Vector2(0, yOffset);
    }

    private void ClearProperties()
    {
        foreach (var field in _uiFields) 
            Destroy(field.Target.gameObject);

        EditorController.CanEdit = true;
        _uiFields.Clear();

        layerText.rectTransform.anchoredPosition = new Vector2(0, 0);
    }
}

}