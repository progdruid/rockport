using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ManipulatorUIPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject propertyFieldPrefab;

    private readonly List<RectTransform> _uiFieldRects = new ();
    
    public event Action<bool> EditingStateChangeEvent;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(canvas);
        Assert.IsNotNull(propertyFieldPrefab);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void CreateUIFields(IEnumerator<PropertyHandle> handles)
    {
        ClearProperties();
        
        while (handles.MoveNext())
        {
            var handle = handles.Current;
            var rect = CreateField(handle);
            _uiFieldRects.Add(rect);
        }

        float yOffset = 0;
        for (var i = _uiFieldRects.Count - 1; i >= 0; i--)
        {
            var rect = _uiFieldRects[i];
            rect.anchoredPosition = new Vector2(0, yOffset);
            yOffset += rect.sizeDelta.y;
        }
    }

    public void ClearProperties()
    {
        foreach (var rect in _uiFieldRects) 
            Destroy(rect.gameObject);
        _uiFieldRects.Clear();
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private RectTransform CreateField(PropertyHandle handle)
    {
        var fieldObject = Instantiate(propertyFieldPrefab, canvas.transform);
        var field = fieldObject.GetComponent<TextPropertyUIField>();
        Assert.IsNotNull(field);
        field.SetProperty(handle);
        if (EditingStateChangeEvent != null) 
            field.EditingStateChangeEvent += EditingStateChangeEvent.Invoke;
        return field.Target;
    }
    
}
