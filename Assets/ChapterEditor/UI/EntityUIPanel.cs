using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class EntityUIPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject propertyFieldPrefab;
    [SerializeField] private RectTransform layerTextPanel;

    private readonly List<TextPropertyUIField> _uiFields = new();
    private IPropertyHolder _propertyHolder = null;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(canvas);
        Assert.IsNotNull(propertyFieldPrefab);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetPropertyHolder(IPropertyHolder propertyHolder)
    {
        _propertyHolder = propertyHolder;
        _propertyHolder.PropertiesChangeEvent += CreateUIFields;
        CreateUIFields();
    }

    public void UnsetPropertyHolder()
    {
        ClearProperties();
        _propertyHolder.PropertiesChangeEvent -= CreateUIFields;
        _propertyHolder = null;
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CreateUIFields()
    {
        ClearProperties();

        Assert.IsNotNull(_propertyHolder);
        var handles = _propertyHolder.GetProperties();
        while (handles.MoveNext())
        {
            var field = Instantiate(propertyFieldPrefab, canvas.transform).GetComponent<TextPropertyUIField>();
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

        layerTextPanel.anchoredPosition = new Vector2(0, yOffset);
    }

    private void ClearProperties()
    {
        foreach (var field in _uiFields)
        {
            Destroy(field.Target.gameObject);
        }

        EditorController.s_CanEdit = true;
        _uiFields.Clear();

        layerTextPanel.anchoredPosition = new Vector2(0, 0);
    }
}

}