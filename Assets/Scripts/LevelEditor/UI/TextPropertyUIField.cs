using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TextPropertyUIField : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private TMP_InputField inputField;
    
    private void Awake()
    {
        Assert.IsNotNull(textField);
        Assert.IsNotNull(inputField);
    }

#if UNITY_EDITOR

    [SerializeField] private string propName;

    private float _propValue = 0;
    
    private void OnValidate()
    {
        var propertyHandler = new TextPropertyHandle()
        {
            PropertyName = propName,
            PropertyDefaultValue = "0",
            TextPropertyType = TextPropertyType.Decimal,
            ChangeEvent = new TMP_InputField.SubmitEvent()
        };
        propertyHandler.ParserSetter = (string val) =>
        {
            _propValue = float.Parse(val);
            propertyHandler.ChangeEvent?.Invoke(_propValue.ToString(CultureInfo.CurrentCulture));
            Debug.Log(_propValue);
        };
        SetProperty(propertyHandler);
    }

#endif
    
    public void SetProperty(TextPropertyHandle handle)
    {
        textField.text = handle.PropertyName + ": ";
        textField.rectTransform.sizeDelta = new Vector2(textField.preferredWidth, textField.rectTransform.sizeDelta.y);
        if (inputField.transform is RectTransform inputRect)
            inputRect.anchoredPosition = new Vector2(textField.preferredWidth, textField.rectTransform.anchoredPosition.y);
        
        inputField.text = handle.PropertyDefaultValue;
        inputField.contentType = handle.TextPropertyType switch
        {
            TextPropertyType.Decimal => TMP_InputField.ContentType.DecimalNumber,
            TextPropertyType.Integer => TMP_InputField.ContentType.IntegerNumber,
            TextPropertyType.Text => TMP_InputField.ContentType.Alphanumeric,
            _ => throw new ArgumentOutOfRangeException()
        };
        inputField.onEndEdit.AddListener(handle.ParserSetter);
        handle.ChangeEvent.AddListener((string text) => inputField.text = text);
    }
}
